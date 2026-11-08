"""Builds the first-person weapon models as detailed hard-surface meshes.

Run from Blender (or with the bpy module):
    blender --background --python Tools/generate_weapon_models.py -- <output Models folder> [Name,Name]
    python3 Tools/generate_weapon_models.py -- <output Models folder> [Name,Name]

The optional second argument builds only the named models (for example
AssaultRifle,RocketLauncher); without it every model is built.

Conventions match the original kit so the game can use the files unchanged:
the barrel points along +Y, up is +Z, width is X, the grip sits below the
origin, and each file is written to <output>/Weapons/<Name>.fbx.

Each gun is assembled from side-profile extrusions (slides, frames, grips,
stocks), strokes (trigger guards, triggers), and turned parts (barrels,
suppressors, scopes), then bevelled, given smooth shading with sharp hard
edges, and triangulated so Unity imports it exactly as built.
"""

import math
import os
import sys

import bpy  # noqa: E402  (bpy must load before bmesh)
import bmesh
from mathutils import Vector

ARGS = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUTPUT_DIR = ARGS[0] if ARGS else os.getcwd()
ONLY = set(ARGS[1].split(",")) if len(ARGS) > 1 else None
SHARP_ANGLE = math.radians(32.0)


# ---------------------------------------------------------------- scene setup

def clear_scene():
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for mesh in list(bpy.data.meshes):
        bpy.data.meshes.remove(mesh)
    for material_data in list(bpy.data.materials):
        bpy.data.materials.remove(material_data)


def material(name, color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.metallic = metallic
    mat.roughness = roughness
    try:
        mat.use_nodes = True
    except Exception:
        pass
    if mat.node_tree is not None:
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf is not None:
            bsdf.inputs["Base Color"].default_value = (*color, 1.0)
            bsdf.inputs["Metallic"].default_value = metallic
            bsdf.inputs["Roughness"].default_value = roughness
    return mat


class Palette:
    def __init__(self, name, accent, frame=(0.03, 0.033, 0.036), metal=(0.13, 0.135, 0.145)):
        self.metal = material(f"{name} Metal", metal, 0.85, 0.32)
        self.frame = material(f"{name} Polymer", frame, 0.05, 0.62)
        self.texture = material(f"{name} Grip Texture", (0.018, 0.02, 0.022), 0.0, 0.92)
        self.dark = material(f"{name} Dark", (0.012, 0.013, 0.015), 0.6, 0.45)
        self.accent = material(f"{name} Accent", accent, 0.25, 0.3)
        self.glass = material(f"{name} Glass", (0.05, 0.12, 0.2), 0.9, 0.05)
        self.rubber = material(f"{name} Rubber", (0.025, 0.025, 0.025), 0.0, 0.95)


def link(obj, mat, bevel, segments):
    bpy.context.collection.objects.link(obj)
    if mat is not None:
        obj.data.materials.append(mat)
    if bevel > 0:
        modifier = obj.modifiers.new("Edge Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = segments
        modifier.limit_method = "ANGLE"
        modifier.angle_limit = math.radians(40)
        modifier.use_clamp_overlap = True
    return obj


# ---------------------------------------------------------------- primitives

def box(name, center, size, mat, rotation=(0.0, 0.0, 0.0), bevel=0.006, segments=2):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = center
    obj.rotation_euler = [math.radians(a) for a in rotation]
    return link(obj, mat, bevel, segments)


def profile(name, points, width, mat, x=0.0, bevel=0.008, segments=2):
    """Extrudes a side-view outline (list of (y, z)) across X."""
    bm = bmesh.new()
    verts = [bm.verts.new((x - width * 0.5, y, z)) for y, z in points]
    face = bm.faces.new(verts)
    result = bmesh.ops.extrude_face_region(bm, geom=[face])
    moved = [e for e in result["geom"] if isinstance(e, bmesh.types.BMVert)]
    bmesh.ops.translate(bm, vec=(width, 0.0, 0.0), verts=moved)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    return link(bpy.data.objects.new(name, mesh), mat, bevel, segments)


def stroke(name, points, thickness, width, mat, x=0.0):
    """A bar of the given thickness following a polyline in the side view."""
    parts = []
    for i in range(len(points) - 1):
        a = Vector((0.0, *points[i]))
        b = Vector((0.0, *points[i + 1]))
        d = b - a
        length = d.length + thickness * 0.9
        angle = math.degrees(math.atan2(d.z, d.y))
        parts.append(box(
            f"{name} {i + 1}",
            (x, (a.y + b.y) * 0.5, (a.z + b.z) * 0.5),
            (width, length, thickness),
            mat,
            rotation=(angle, 0.0, 0.0),
            bevel=thickness * 0.3,
        ))
    return parts


def tube(name, y0, y1, radius, mat, z=0.0, x=0.0, radius_end=None, vertices=28, bevel=0.004):
    """A cylinder (or cone) whose axis runs along Y from y0 to y1."""
    bm = bmesh.new()
    r1 = radius
    r2 = radius if radius_end is None else radius_end
    bmesh.ops.create_cone(
        bm,
        cap_ends=True,
        cap_tris=False,
        segments=vertices,
        radius1=r1,
        radius2=r2,
        depth=abs(y1 - y0),
    )
    # create_cone builds along Z; turn it so +Z maps to +Y.
    for v in bm.verts:
        v.co = Vector((v.co.x, v.co.z, -v.co.y))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = (x, (y0 + y1) * 0.5, z)
    if y1 < y0:
        obj.rotation_euler = (0.0, 0.0, math.pi)
    return link(obj, mat, bevel, 1)


def tube_x(name, x0, x1, radius, mat, y, z, vertices=20):
    """A cylinder whose axis runs along X."""
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=vertices,
                          radius1=radius, radius2=radius, depth=abs(x1 - x0))
    for v in bm.verts:
        v.co = Vector((v.co.z, v.co.y, -v.co.x))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = ((x0 + x1) * 0.5, y, z)
    return link(obj, mat, 0.003, 1)


def tube_z(name, z0, z1, radius, mat, x, y, vertices=20):
    """A cylinder whose axis runs along Z."""
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=vertices,
                          radius1=radius, radius2=radius, depth=abs(z1 - z0))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = (x, y, (z0 + z1) * 0.5)
    return link(obj, mat, 0.003, 1)


def sphere(name, center, radius, mat):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=radius)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = center
    return link(obj, mat, 0.0, 1)


def ribs(name, y_start, y_end, count, center_x, z, size, mat):
    """Evenly spaced ridges (rail teeth, serrations, vents)."""
    for i in range(count):
        t = i / max(1, count - 1)
        y = y_start + (y_end - y_start) * t
        box(f"{name} {i + 1}", (center_x, y, z), size, mat, bevel=0.002, segments=1)


def picatinny(name, y0, y1, z, width, mat):
    length = y1 - y0
    box(f"{name} Base", (0.0, (y0 + y1) * 0.5, z), (width, length, 0.02), mat, bevel=0.003)
    teeth = max(2, int(length / 0.042))
    ribs(f"{name} Tooth", y0 + 0.015, y1 - 0.015, teeth, 0.0, z + 0.016, (width + 0.012, 0.02, 0.014), mat)


# ---------------------------------------------------------------- finishing

def finish_all():
    """Applies modifiers, marks hard edges sharp, smooth-shades, triangulates."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    for obj in [o for o in bpy.data.objects if o.type == "MESH"]:
        evaluated = obj.evaluated_get(depsgraph)
        mesh = bpy.data.meshes.new_from_object(evaluated)
        old = obj.data
        obj.modifiers.clear()
        obj.data = mesh
        mesh.name = obj.name
        bpy.data.meshes.remove(old)

        bm = bmesh.new()
        bm.from_mesh(mesh)
        for face in bm.faces:
            face.smooth = True
        for edge in bm.edges:
            if len(edge.link_faces) == 2:
                edge.smooth = edge.calc_face_angle(0.0) < SHARP_ANGLE
            else:
                edge.smooth = False
        bmesh.ops.triangulate(bm, faces=bm.faces[:], quad_method="BEAUTY", ngon_method="BEAUTY")
        bm.to_mesh(mesh)
        bm.free()


def export_model(name):
    finish_all()
    folder = os.path.join(OUTPUT_DIR, "Weapons")
    os.makedirs(folder, exist_ok=True)
    for obj in bpy.data.objects:
        obj.select_set(obj.type == "MESH")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(folder, f"{name}.fbx"),
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="OFF",
        bake_anim=False,
        add_leaf_bones=False,
        path_mode="AUTO",
    )
    tris = sum(len(o.data.polygons) for o in bpy.data.objects if o.type == "MESH")
    print(f"Exported {name}: {len(bpy.data.objects)} parts, {tris} triangles")


# ---------------------------------------------------------------- shared parts

def pistol_frame(p, slide_front=0.5, grip_colour=None):
    """Slide, frame, raked grip, trigger group, and sights for the pistol."""
    # Slide with a chamfered nose and a slightly sloped rear.
    profile("Slide", [
        (-0.30, 0.10), (slide_front, 0.10), (slide_front, 0.215),
        (slide_front - 0.035, 0.27), (-0.275, 0.27), (-0.30, 0.245),
    ], 0.124, p.metal, bevel=0.012, segments=3)

    # Rear and front cocking serrations.
    ribs("Rear Serration", -0.27, -0.13, 7, 0.0, 0.19, (0.128, 0.011, 0.12), p.dark)
    ribs("Front Serration", slide_front - 0.2, slide_front - 0.09, 5, 0.0, 0.19, (0.128, 0.011, 0.1), p.dark)

    # Ejection port and extractor on the right side.
    box("Ejection Port", (0.061, 0.08, 0.225), (0.008, 0.15, 0.06), p.dark, bevel=0.003)
    box("Extractor", (0.063, -0.01, 0.2), (0.006, 0.05, 0.016), p.metal, bevel=0.002)

    # Polymer frame with the dust cover and accessory rail.
    profile("Frame", [
        (-0.285, 0.105), (slide_front - 0.03, 0.105), (slide_front - 0.03, 0.05),
        (0.17, 0.04), (0.14, 0.05), (-0.2, 0.05),
    ], 0.116, p.frame, bevel=0.008)
    ribs("Frame Rail Tooth", 0.25, slide_front - 0.07, 4, 0.0, 0.046, (0.08, 0.018, 0.014), p.frame)

    # Raked grip with finger grooves and a beavertail.
    grip_mat = grip_colour or p.frame
    grip_outline = [
        (0.03, 0.06), (0.02, 0.0), (0.0, -0.055), (-0.004, -0.085), (-0.016, -0.12),
        (-0.02, -0.15), (-0.032, -0.185), (-0.037, -0.215), (-0.05, -0.275), (-0.062, -0.295),
        (-0.19, -0.295), (-0.205, -0.275), (-0.18, -0.12), (-0.17, -0.01), (-0.2, 0.045),
        (-0.3, 0.08), (-0.29, 0.105), (-0.16, 0.105),
    ]
    profile("Grip", grip_outline, 0.112, grip_mat, bevel=0.012, segments=3)

    # Stippled grip panels standing just proud of the grip sides.
    panel = [(-0.035, -0.04), (-0.05, -0.255), (-0.175, -0.255), (-0.158, -0.04)]
    for side in (-1, 1):
        profile(f"Grip Texture Panel {side}", panel, 0.006, p.texture, x=side * 0.057, bevel=0.002, segments=1)

    # Magazine base plate.
    profile("Magazine Base Plate", [(-0.052, -0.29), (-0.057, -0.325), (-0.205, -0.325), (-0.205, -0.29)],
            0.104, p.dark, bevel=0.006)

    # Trigger guard and trigger.
    stroke("Trigger Guard", [(0.165, 0.045), (0.17, 0.0), (0.15, -0.04), (0.11, -0.058), (0.0, -0.058), (-0.01, -0.04)],
           0.03, 0.066, p.frame)
    stroke("Trigger", [(0.075, 0.045), (0.08, 0.01), (0.072, -0.018), (0.055, -0.032)], 0.02, 0.03, p.metal)

    # Controls: slide stop on the left, takedown lever, magazine release.
    box("Slide Stop Lever", (-0.063, 0.03, 0.115), (0.01, 0.1, 0.02), p.dark, bevel=0.003)
    box("Takedown Lever", (-0.061, 0.12, 0.085), (0.008, 0.035, 0.018), p.dark, bevel=0.003)
    box("Magazine Release", (-0.06, -0.005, 0.035), (0.012, 0.035, 0.03), p.dark, bevel=0.003)

    # Sights: three-dot setup.
    box("Front Sight Post", (0.0, slide_front - 0.05, 0.285), (0.018, 0.03, 0.03), p.dark, bevel=0.003)
    box("Front Sight Dot", (0.0, slide_front - 0.064, 0.29), (0.008, 0.003, 0.008), p.accent, bevel=0.0)
    for side in (-1, 1):
        box(f"Rear Sight Post {side}", (side * 0.033, -0.245, 0.287), (0.03, 0.04, 0.035), p.dark, bevel=0.004)
        box(f"Rear Sight Dot {side}", (side * 0.033, -0.226, 0.292), (0.008, 0.003, 0.008), p.accent, bevel=0.0)


# ---------------------------------------------------------------- the guns

def tranquilizer_pistol():
    clear_scene()
    p = Palette("TranquilizerPistol", (0.1, 0.82, 0.72))
    pistol_frame(p, slide_front=0.48)

    # Long, thin dart barrel running past the slide, with a teal muzzle collar.
    tube("Dart Barrel", 0.42, 0.66, 0.026, p.metal, z=0.205)
    tube("Barrel Muzzle Collar", 0.62, 0.665, 0.032, p.accent, z=0.205)
    tube("Barrel Bore", 0.664, 0.668, 0.016, p.dark, z=0.205, bevel=0.0)
    tube("Barrel Support Band", 0.5, 0.52, 0.031, p.metal, z=0.205)

    # Sedative cartridge under the frame: glass tube with the dose visible.
    tube("Sedative Cartridge Glass", 0.2, 0.44, 0.024, p.glass, z=0.015)
    tube("Sedative Cartridge Dose", 0.215, 0.39, 0.018, p.accent, z=0.015)
    tube("Sedative Cartridge Rear Cap", 0.185, 0.215, 0.027, p.metal, z=0.015)
    tube("Sedative Cartridge Front Cap", 0.43, 0.46, 0.027, p.metal, z=0.015)
    box("Sedative Cartridge Mount", (0.0, 0.32, 0.035), (0.03, 0.2, 0.02), p.frame, bevel=0.004)

    # CO2 cap at the base of the grip and a teal stripe along the slide.
    tube_z("CO2 Cap", -0.35, -0.325, 0.03, p.accent, x=0.0, y=-0.13)
    for side in (-1, 1):
        box(f"Slide Stripe {side}", (side * 0.0625, 0.18, 0.25), (0.003, 0.36, 0.01), p.accent, bevel=0.0)
    export_model("TranquilizerPistol")


def sedative_smg():
    clear_scene()
    p = Palette("SedativeSMG", (0.75, 0.16, 0.22))

    # Upper receiver and handguard.
    profile("Upper Receiver", [
        (-0.36, 0.1), (0.3, 0.1), (0.3, 0.28), (-0.32, 0.28), (-0.36, 0.25),
    ], 0.13, p.metal, bevel=0.01, segments=3)
    profile("Handguard", [
        (0.29, 0.085), (0.52, 0.085), (0.54, 0.11), (0.54, 0.26), (0.52, 0.28), (0.29, 0.28),
    ], 0.145, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        ribs(f"Handguard Vent {side}", 0.33, 0.49, 5, side * 0.0725, 0.19, (0.006, 0.022, 0.09), p.dark)
    picatinny("Top Rail", -0.32, 0.53, 0.29, 0.085, p.metal)
    picatinny("Bottom Rail", 0.33, 0.5, 0.075, 0.07, p.metal)

    # Suppressor for the dart round.
    tube("Barrel", 0.53, 0.6, 0.028, p.metal, z=0.19)
    tube("Suppressor Body", 0.58, 0.84, 0.052, p.dark, z=0.19)
    tube("Suppressor Rear Collar", 0.575, 0.6, 0.056, p.metal, z=0.19)
    tube("Suppressor End Cap", 0.82, 0.845, 0.056, p.metal, z=0.19)
    tube("Suppressor Band", 0.7, 0.72, 0.055, p.accent, z=0.19)
    tube("Suppressor Bore", 0.844, 0.848, 0.016, p.dark, z=0.19, bevel=0.0)

    # Lower receiver, magazine well, and curved dart magazine.
    profile("Lower Receiver", [
        (-0.3, 0.105), (0.24, 0.105), (0.24, 0.055), (0.16, 0.035), (-0.1, 0.035), (-0.3, 0.06),
    ], 0.12, p.frame, bevel=0.008)
    profile("Magazine", [
        (0.02, 0.05), (0.13, 0.05), (0.16, -0.12), (0.22, -0.32), (0.11, -0.355), (0.06, -0.16),
    ], 0.085, p.frame, bevel=0.01, segments=3)
    profile("Magazine Base Plate", [(0.11, -0.345), (0.225, -0.31), (0.235, -0.335), (0.12, -0.375)],
            0.095, p.accent, bevel=0.005)
    for side in (-1, 1):
        box(f"Magazine Window {side}", (side * 0.0435, 0.09, -0.08), (0.004, 0.035, 0.16), p.glass, rotation=(-8, 0, 0), bevel=0.0)

    # Pistol grip, trigger group, and controls.
    profile("Pistol Grip", [
        (-0.08, 0.04), (-0.095, -0.03), (-0.11, -0.08), (-0.115, -0.12), (-0.13, -0.17),
        (-0.15, -0.28), (-0.27, -0.28), (-0.245, -0.1), (-0.22, 0.04),
    ], 0.105, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        profile(f"Grip Texture Panel {side}", [(-0.11, -0.03), (-0.14, -0.24), (-0.25, -0.24), (-0.225, -0.03)],
                0.006, p.texture, x=side * 0.053, bevel=0.002, segments=1)
    stroke("Trigger Guard", [(0.0, 0.035), (0.01, -0.03), (-0.01, -0.06), (-0.09, -0.06)], 0.02, 0.06, p.frame)
    stroke("Trigger", [(-0.035, 0.035), (-0.03, 0.0), (-0.04, -0.03)], 0.016, 0.025, p.metal)
    box("Selector Switch", (-0.066, -0.17, 0.085), (0.01, 0.05, 0.02), p.accent, bevel=0.003)
    box("Charging Handle", (-0.075, -0.24, 0.235), (0.03, 0.06, 0.03), p.dark, bevel=0.006)
    box("Ejection Port", (0.066, 0.02, 0.2), (0.006, 0.14, 0.06), p.dark, bevel=0.003)

    # Vertical foregrip.
    profile("Foregrip", [(0.36, 0.07), (0.44, 0.07), (0.43, -0.13), (0.42, -0.15), (0.37, -0.15), (0.355, -0.13)],
            0.07, p.frame, bevel=0.015, segments=3)
    box("Foregrip Cap", (0.0, 0.395, -0.158), (0.072, 0.075, 0.02), p.rubber, bevel=0.006)

    # Collapsible stock: twin rods and a butt plate.
    for side in (-1, 1):
        tube(f"Stock Rod {side}", -0.64, -0.34, 0.013, p.metal, z=0.2, x=side * 0.04, vertices=14)
    tube("Stock Lower Rod", -0.64, -0.34, 0.013, p.metal, z=0.115, x=0.0, vertices=14)
    profile("Butt Plate", [(-0.6, 0.06), (-0.6, 0.29), (-0.65, 0.29), (-0.68, 0.24), (-0.68, 0.02), (-0.64, 0.0)],
            0.11, p.frame, bevel=0.01, segments=3)
    box("Butt Pad", (0.0, -0.687, 0.145), (0.1, 0.02, 0.26), p.rubber, bevel=0.008)

    # Red-dot sight.
    box("Red Dot Mount", (0.0, 0.03, 0.315), (0.07, 0.11, 0.025), p.metal, bevel=0.004)
    profile("Red Dot Housing", [(-0.02, 0.325), (0.09, 0.325), (0.08, 0.41), (0.0, 0.415)], 0.075, p.dark, bevel=0.01, segments=3)
    tube("Red Dot Lens", -0.005, 0.07, 0.027, p.glass, z=0.37, vertices=24)
    box("Red Dot Reticle", (0.0, 0.03, 0.37), (0.006, 0.002, 0.006), p.accent, bevel=0.0)
    export_model("SedativeSMG")


def precision_rifle():
    clear_scene()
    p = Palette("PrecisionRifle", (0.12, 0.56, 0.88), frame=(0.17, 0.19, 0.15))

    # Chassis stock: forend, action bed, and a buttstock with a raised comb.
    profile("Stock Chassis", [
        (0.72, 0.17), (0.72, 0.09), (0.66, 0.055), (0.2, 0.055), (0.12, 0.04),
        (-0.3, 0.04), (-0.44, 0.02), (-0.98, -0.1), (-1.02, -0.085), (-1.02, 0.27),
        (-0.98, 0.3), (-0.62, 0.29), (-0.52, 0.245), (-0.42, 0.17),
    ], 0.105, p.frame, bevel=0.014, segments=3)
    box("Cheek Riser", (0.0, -0.76, 0.312), (0.08, 0.3, 0.035), p.accent, bevel=0.01, segments=2)
    box("Butt Pad", (0.0, -1.033, 0.09), (0.11, 0.025, 0.37), p.rubber, bevel=0.01)
    for side in (-1, 1):
        ribs(f"Forend Vent {side}", 0.28, 0.62, 6, side * 0.0535, 0.11, (0.005, 0.03, 0.05), p.dark)

    # Pistol grip and trigger group.
    profile("Pistol Grip", [
        (-0.2, 0.05), (-0.215, -0.02), (-0.235, -0.07), (-0.24, -0.11), (-0.26, -0.21),
        (-0.37, -0.21), (-0.36, -0.08), (-0.33, 0.04),
    ], 0.1, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        profile(f"Grip Texture Panel {side}", [(-0.235, -0.02), (-0.255, -0.18), (-0.355, -0.18), (-0.335, -0.02)],
                0.006, p.texture, x=side * 0.051, bevel=0.002, segments=1)
    stroke("Trigger Guard", [(-0.07, 0.04), (-0.06, -0.025), (-0.09, -0.05), (-0.2, -0.05)], 0.02, 0.06, p.metal)
    stroke("Trigger", [(-0.135, 0.04), (-0.13, 0.005), (-0.14, -0.022)], 0.015, 0.024, p.metal)

    # Detachable box magazine.
    profile("Magazine", [(-0.04, 0.05), (0.1, 0.05), (0.09, -0.11), (-0.03, -0.11)], 0.075, p.metal, bevel=0.008)
    box("Magazine Base Plate", (0.0, 0.03, -0.12), (0.082, 0.14, 0.02), p.accent, bevel=0.004)

    # Round receiver with a bolt and a scope rail.
    tube("Receiver", -0.32, 0.22, 0.058, p.metal, z=0.2)
    box("Receiver Bed", (0.0, -0.05, 0.15), (0.1, 0.54, 0.05), p.metal, bevel=0.006)
    box("Ejection Port", (0.055, -0.02, 0.215), (0.012, 0.16, 0.05), p.dark, bevel=0.003)
    tube("Bolt Shroud", -0.4, -0.32, 0.04, p.metal, z=0.2)
    tube_x("Bolt Handle Arm", 0.04, 0.15, 0.011, p.metal, y=-0.19, z=0.19)
    sphere("Bolt Knob", (0.165, -0.19, 0.175), 0.024, p.dark)
    picatinny("Scope Rail", -0.3, 0.2, 0.265, 0.07, p.metal)

    # Fluted barrel and suppressor.
    tube("Barrel", 0.2, 0.97, 0.034, p.metal, z=0.2, radius_end=0.026)
    for i, y in enumerate((0.35, 0.5, 0.65, 0.8)):
        tube(f"Barrel Flute Ring {i + 1}", y - 0.006, y + 0.006, 0.031, p.dark, z=0.2)
    tube("Suppressor Body", 0.95, 1.24, 0.048, p.dark, z=0.2)
    tube("Suppressor Rear Collar", 0.945, 0.975, 0.052, p.metal, z=0.2)
    tube("Suppressor End Cap", 1.22, 1.245, 0.052, p.metal, z=0.2)
    tube("Suppressor Band", 1.08, 1.1, 0.051, p.accent, z=0.2)
    tube("Suppressor Bore", 1.244, 1.248, 0.014, p.dark, z=0.2, bevel=0.0)

    # Scope: eyepiece, tube, turrets, objective bell, rings.
    scope_z = 0.37
    tube("Scope Eyepiece", -0.5, -0.38, 0.048, p.dark, z=scope_z)
    tube("Scope Eyepiece Taper", -0.38, -0.3, 0.048, p.dark, z=scope_z, radius_end=0.034)
    tube("Scope Tube", -0.3, 0.24, 0.034, p.dark, z=scope_z)
    tube("Scope Objective Taper", 0.24, 0.34, 0.034, p.dark, z=scope_z, radius_end=0.056)
    tube("Scope Objective Bell", 0.34, 0.47, 0.056, p.dark, z=scope_z)
    tube("Scope Objective Lens", 0.468, 0.472, 0.048, p.glass, z=scope_z, bevel=0.0)
    tube("Scope Ocular Lens", -0.502, -0.498, 0.04, p.glass, z=scope_z, bevel=0.0)
    tube("Scope Focus Ring", -0.46, -0.43, 0.051, p.rubber, z=scope_z)
    tube_z("Scope Elevation Turret", scope_z + 0.03, scope_z + 0.085, 0.024, p.metal, x=0.0, y=-0.03)
    tube_z("Scope Elevation Cap", scope_z + 0.085, scope_z + 0.095, 0.026, p.accent, x=0.0, y=-0.03)
    tube_x("Scope Windage Turret", 0.03, 0.085, 0.022, p.metal, y=-0.03, z=scope_z)
    for i, y in enumerate((-0.2, 0.12)):
        tube(f"Scope Ring {i + 1}", y - 0.018, y + 0.018, 0.042, p.metal, z=scope_z)
        box(f"Scope Ring Base {i + 1}", (0.0, y, (scope_z + 0.285) * 0.5 - 0.01), (0.07, 0.05, scope_z - 0.285), p.metal, bevel=0.006)

    # Folded bipod under the forend.
    box("Bipod Pivot", (0.0, 0.64, 0.035), (0.07, 0.05, 0.03), p.metal, bevel=0.006)
    for side in (-1, 1):
        tube(f"Bipod Leg {side}", 0.32, 0.63, 0.011, p.metal, z=0.02, x=side * 0.028, vertices=12)
        box(f"Bipod Foot {side}", (side * 0.028, 0.31, 0.02), (0.022, 0.03, 0.026), p.rubber, bevel=0.006)
    export_model("PrecisionRifle")


def assault_rifle():
    clear_scene()
    p = Palette("AssaultRifle", (0.9, 0.46, 0.12), frame=(0.3, 0.26, 0.19))

    # Flat-top upper receiver and a long free-float handguard with slots.
    profile("Upper Receiver", [
        (-0.36, 0.1), (0.26, 0.1), (0.26, 0.27), (-0.33, 0.27), (-0.36, 0.24),
    ], 0.12, p.metal, bevel=0.01, segments=3)
    box("Forward Assist", (0.066, -0.2, 0.2), (0.03, 0.06, 0.04), p.metal, bevel=0.006)
    box("Ejection Port Cover", (0.062, -0.02, 0.19), (0.006, 0.17, 0.055), p.dark, bevel=0.003)
    box("Brass Deflector", (0.068, -0.13, 0.245), (0.02, 0.04, 0.04), p.metal, bevel=0.006)
    profile("Handguard", [
        (0.25, 0.085), (0.8, 0.085), (0.82, 0.105), (0.82, 0.265), (0.8, 0.285), (0.25, 0.285),
    ], 0.13, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        ribs(f"Handguard Slot {side}", 0.32, 0.76, 7, side * 0.0655, 0.185, (0.006, 0.045, 0.02), p.dark)
    ribs("Handguard Bottom Slot", 0.32, 0.76, 7, 0.0, 0.082, (0.04, 0.045, 0.006), p.dark)
    picatinny("Top Rail", -0.33, 0.8, 0.295, 0.08, p.metal)

    # Barrel, gas block, and a slotted muzzle brake.
    tube("Barrel", 0.8, 1.06, 0.022, p.metal, z=0.185)
    box("Gas Block", (0.0, 0.84, 0.185), (0.05, 0.04, 0.06), p.metal, bevel=0.006)
    tube("Muzzle Brake", 1.04, 1.15, 0.032, p.dark, z=0.185)
    for side in (-1, 1):
        ribs(f"Muzzle Brake Port {side}", 1.07, 1.12, 3, side * 0.03, 0.185, (0.008, 0.012, 0.03), p.metal)
    tube("Muzzle Bore", 1.149, 1.153, 0.012, p.dark, z=0.185, bevel=0.0)

    # Lower receiver, magazine well, and a curved steel magazine.
    profile("Lower Receiver", [
        (-0.33, 0.105), (0.22, 0.105), (0.22, 0.04), (0.14, 0.02), (-0.08, 0.02), (-0.33, 0.06),
    ], 0.11, p.metal, bevel=0.008)
    profile("Magazine", [
        (0.02, 0.04), (0.14, 0.04), (0.16, -0.1), (0.24, -0.3), (0.13, -0.34), (0.06, -0.14),
    ], 0.08, p.dark, bevel=0.01, segments=3)
    for side in (-1, 1):
        ribs(f"Magazine Rib {side}", -0.05, -0.25, 4, side * 0.041, 0.0, (0.004, 0.02, 0.01), p.metal)
    profile("Magazine Base Plate", [(0.13, -0.33), (0.245, -0.29), (0.255, -0.315), (0.14, -0.36)],
            0.09, p.accent, bevel=0.005)

    # Pistol grip, trigger group, and controls.
    profile("Pistol Grip", [
        (-0.1, 0.03), (-0.115, -0.03), (-0.13, -0.08), (-0.135, -0.12), (-0.15, -0.17),
        (-0.17, -0.28), (-0.29, -0.28), (-0.265, -0.1), (-0.24, 0.03),
    ], 0.1, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        profile(f"Grip Texture Panel {side}", [(-0.13, -0.03), (-0.16, -0.24), (-0.27, -0.24), (-0.245, -0.03)],
                0.006, p.texture, x=side * 0.051, bevel=0.002, segments=1)
    stroke("Trigger Guard", [(0.0, 0.025), (0.01, -0.035), (-0.02, -0.06), (-0.11, -0.06)], 0.02, 0.06, p.metal)
    stroke("Trigger", [(-0.05, 0.025), (-0.045, -0.005), (-0.055, -0.03)], 0.016, 0.025, p.metal)
    box("Selector Lever", (-0.06, -0.19, 0.07), (0.01, 0.05, 0.022), p.accent, bevel=0.003)
    box("Magazine Release", (0.058, 0.06, 0.06), (0.012, 0.03, 0.03), p.dark, bevel=0.003)
    box("Charging Handle", (0.0, -0.37, 0.25), (0.08, 0.04, 0.025), p.dark, bevel=0.005)

    # Buffer tube and an adjustable stock.
    tube("Buffer Tube", -0.75, -0.34, 0.032, p.metal, z=0.19, vertices=20)
    profile("Stock", [
        (-0.58, 0.24), (-0.58, 0.13), (-0.66, 0.02), (-0.9, -0.01), (-0.93, 0.02), (-0.93, 0.25), (-0.88, 0.27), (-0.62, 0.27),
    ], 0.09, p.frame, bevel=0.012, segments=3)
    box("Butt Pad", (0.0, -0.94, 0.13), (0.095, 0.02, 0.26), p.rubber, bevel=0.008)
    box("Stock Lock Lever", (0.0, -0.66, 0.015), (0.03, 0.08, 0.02), p.dark, bevel=0.004)

    # Holographic sight and a folding rear sight.
    box("Holo Sight Base", (0.0, 0.02, 0.32), (0.075, 0.15, 0.03), p.dark, bevel=0.005)
    profile("Holo Sight Hood", [(-0.05, 0.335), (0.09, 0.335), (0.09, 0.43), (-0.05, 0.43)], 0.075, p.dark, bevel=0.01, segments=2)
    box("Holo Sight Window", (0.0, 0.02, 0.385), (0.06, 0.13, 0.07), p.glass, bevel=0.0)
    box("Holo Sight Reticle", (0.0, 0.03, 0.385), (0.006, 0.002, 0.006), p.accent, bevel=0.0)
    box("Rear Flip Sight", (0.0, -0.27, 0.32), (0.06, 0.03, 0.03), p.metal, bevel=0.004)
    box("Front Flip Sight", (0.0, 0.76, 0.32), (0.05, 0.03, 0.035), p.metal, bevel=0.004)

    # Angled foregrip.
    profile("Angled Foregrip", [(0.42, 0.08), (0.6, 0.08), (0.56, 0.0), (0.46, -0.03)], 0.06, p.frame, bevel=0.01, segments=3)
    export_model("AssaultRifle")


def grenade_launcher():
    clear_scene()
    p = Palette("GrenadeLauncher", (0.95, 0.75, 0.1), frame=(0.2, 0.22, 0.16))
    drum_z = 0.08
    bore_z = drum_z + 0.095

    # Revolving six-round drum with chamber mouths on the front face.
    tube("Drum Body", -0.1, 0.2, 0.15, p.frame, z=drum_z, vertices=36)
    tube("Drum Rear Plate", -0.13, -0.1, 0.155, p.metal, z=drum_z, vertices=36)
    tube("Drum Front Plate", 0.2, 0.225, 0.155, p.metal, z=drum_z, vertices=36)
    for i in range(6):
        a = i * math.pi / 3.0
        x = math.sin(a) * 0.095
        z = drum_z + math.cos(a) * 0.095
        tube(f"Chamber Mouth {i + 1}", 0.224, 0.229, 0.038, p.dark, z=z, x=x, bevel=0.0, vertices=20)
        tube(f"Drum Flute {i + 1}", -0.08, 0.18, 0.02, p.dark, z=drum_z + math.cos(a + math.pi / 6) * 0.148,
             x=math.sin(a + math.pi / 6) * 0.148, vertices=12)
    tube("Drum Axle", 0.22, 0.3, 0.025, p.metal, z=drum_z)

    # Wide barrel with grip bands and a flared muzzle.
    tube("Barrel", 0.225, 0.82, 0.06, p.metal, z=bore_z, vertices=32)
    tube("Barrel Muzzle", 0.8, 0.86, 0.066, p.dark, z=bore_z, vertices=32)
    tube("Barrel Bore", 0.859, 0.863, 0.044, p.dark, z=bore_z, bevel=0.0, vertices=32)
    for i, y in enumerate((0.36, 0.48, 0.6)):
        tube(f"Barrel Band {i + 1}", y - 0.012, y + 0.012, 0.064, p.accent if i == 1 else p.dark, z=bore_z, vertices=32)

    # Top strap from the frame over the drum to the barrel, with a rail and leaf sight.
    box("Top Strap", (0.0, 0.08, bore_z + 0.085), (0.06, 0.62, 0.035), p.metal, bevel=0.006)
    picatinny("Top Rail", -0.12, 0.3, bore_z + 0.115, 0.06, p.metal)
    box("Leaf Sight Base", (0.0, 0.4, bore_z + 0.08), (0.05, 0.06, 0.03), p.metal, bevel=0.004)
    box("Leaf Sight Plate", (0.0, 0.4, bore_z + 0.13), (0.06, 0.008, 0.08), p.dark, rotation=(-15, 0, 0), bevel=0.002)

    # Rear frame, pistol grip, and trigger group.
    profile("Rear Frame", [(-0.36, 0.06), (-0.12, 0.06), (-0.12, 0.29), (-0.33, 0.29), (-0.36, 0.25)], 0.11, p.metal, bevel=0.01, segments=3)
    profile("Pistol Grip", [
        (-0.2, 0.06), (-0.215, -0.01), (-0.23, -0.06), (-0.235, -0.1), (-0.25, -0.15),
        (-0.27, -0.26), (-0.38, -0.26), (-0.36, -0.08), (-0.33, 0.06),
    ], 0.1, p.frame, bevel=0.012, segments=3)
    stroke("Trigger Guard", [(-0.08, 0.06), (-0.07, -0.01), (-0.1, -0.04), (-0.2, -0.04)], 0.022, 0.06, p.metal)
    stroke("Trigger", [(-0.135, 0.06), (-0.13, 0.025), (-0.14, -0.002)], 0.016, 0.026, p.metal)

    # Folding stock on a tube.
    tube("Stock Tube", -0.7, -0.36, 0.028, p.metal, z=0.19, vertices=18)
    profile("Stock", [(-0.56, 0.24), (-0.56, 0.12), (-0.66, 0.0), (-0.84, -0.02), (-0.86, 0.01), (-0.86, 0.26), (-0.6, 0.27)],
            0.085, p.frame, bevel=0.012, segments=3)
    box("Butt Pad", (0.0, -0.87, 0.12), (0.09, 0.02, 0.27), p.rubber, bevel=0.008)

    # Forward grip under the barrel.
    profile("Foregrip", [(0.42, bore_z - 0.06), (0.5, bore_z - 0.06), (0.49, -0.16), (0.48, -0.18), (0.43, -0.18), (0.415, -0.16)],
            0.065, p.frame, bevel=0.014, segments=3)
    export_model("GrenadeLauncher")


def rocket_launcher():
    clear_scene()
    p = Palette("RocketLauncher", (0.85, 0.16, 0.12), frame=(0.22, 0.25, 0.17))
    tube_z_axis = 0.16

    # Launch tube with heat-shield wraps, a flared rear venturi, and a front collar.
    tube("Launch Tube", -0.7, 0.62, 0.075, p.frame, z=tube_z_axis, vertices=36)
    tube("Rear Venturi", -0.9, -0.7, 0.11, p.dark, z=tube_z_axis, radius_end=0.075, vertices=36)
    tube("Rear Venturi Lip", -0.92, -0.9, 0.112, p.metal, z=tube_z_axis, vertices=36)
    tube("Front Collar", 0.6, 0.66, 0.082, p.metal, z=tube_z_axis, vertices=36)
    for i, y in enumerate((-0.1, 0.05, 0.2)):
        tube(f"Heat Shield Wrap {i + 1}", y - 0.06, y + 0.06, 0.08, p.texture, z=tube_z_axis, vertices=36)

    # Loaded rocket warhead sticking out of the front.
    tube("Warhead Neck", 0.66, 0.74, 0.045, p.metal, z=tube_z_axis, vertices=24)
    tube("Warhead Body", 0.74, 0.92, 0.085, p.frame, z=tube_z_axis, vertices=32)
    tube("Warhead Band", 0.86, 0.88, 0.087, p.accent, z=tube_z_axis, vertices=32)
    tube("Warhead Cone", 0.92, 1.1, 0.085, p.frame, z=tube_z_axis, radius_end=0.02, vertices=32)
    tube("Warhead Fuze Tip", 1.1, 1.14, 0.02, p.metal, z=tube_z_axis, radius_end=0.008, vertices=16)

    # Pistol grip with trigger, and a forward grip.
    box("Grip Mount", (0.0, 0.08, tube_z_axis - 0.085), (0.06, 0.24, 0.03), p.metal, bevel=0.005)
    profile("Pistol Grip", [
        (0.04, 0.06), (0.03, 0.0), (0.015, -0.05), (0.01, -0.09), (-0.005, -0.14),
        (-0.025, -0.24), (-0.13, -0.24), (-0.11, -0.06), (-0.09, 0.06),
    ], 0.095, p.frame, bevel=0.012, segments=3)
    stroke("Trigger Guard", [(0.15, 0.06), (0.16, 0.0), (0.13, -0.03), (0.04, -0.03)], 0.02, 0.06, p.metal)
    stroke("Trigger", [(0.1, 0.06), (0.105, 0.025), (0.095, -0.002)], 0.016, 0.026, p.metal)
    profile("Forward Grip", [(0.34, 0.07), (0.42, 0.07), (0.41, -0.13), (0.4, -0.15), (0.35, -0.15), (0.335, -0.13)],
            0.065, p.frame, bevel=0.014, segments=3)
    box("Forward Grip Mount", (0.0, 0.38, tube_z_axis - 0.085), (0.06, 0.12, 0.03), p.metal, bevel=0.005)

    # Shoulder rest under the rear of the tube.
    profile("Shoulder Rest", [(-0.55, 0.08), (-0.3, 0.08), (-0.34, 0.02), (-0.5, 0.0), (-0.56, 0.03)], 0.1, p.rubber, bevel=0.012, segments=3)

    # Optical sight on the left, on a bracket.
    box("Sight Bracket", (-0.09, 0.14, tube_z_axis + 0.06), (0.03, 0.1, 0.06), p.metal, bevel=0.005)
    tube("Sight Body", 0.02, 0.28, 0.03, p.dark, z=tube_z_axis + 0.1, x=-0.12, vertices=20)
    tube("Sight Eyecup", -0.04, 0.02, 0.036, p.rubber, z=tube_z_axis + 0.1, x=-0.12, vertices=20)
    tube("Sight Lens", 0.279, 0.283, 0.026, p.glass, z=tube_z_axis + 0.1, x=-0.12, bevel=0.0, vertices=20)

    # Iron sights on top and a carry sling loop.
    box("Front Iron Sight", (0.0, 0.5, tube_z_axis + 0.1), (0.03, 0.02, 0.05), p.metal, bevel=0.003)
    box("Rear Iron Sight", (0.0, -0.2, tube_z_axis + 0.1), (0.05, 0.02, 0.045), p.metal, bevel=0.003)
    tube_x("Sling Loop", -0.03, 0.03, 0.01, p.metal, y=-0.45, z=tube_z_axis + 0.085)
    export_model("RocketLauncher")


def flat_bar(name, a, b, thickness, depth, mat, z):
    """A bar lying in the top (X-Y) view from point a to point b at height z."""
    dx, dy = b[0] - a[0], b[1] - a[1]
    length = math.hypot(dx, dy)
    angle = math.degrees(math.atan2(dy, dx))
    return box(name, ((a[0] + b[0]) * 0.5, (a[1] + b[1]) * 0.5, z), (length, thickness, depth), mat,
               rotation=(0.0, 0.0, angle), bevel=min(thickness, depth) * 0.3)


def crossbow():
    clear_scene()
    p = Palette("Crossbow", (1.0, 0.45, 0.08), frame=(0.1, 0.11, 0.1))
    rail_z = 0.2

    # Stock with a thumbhole-style grip, cheek rest, and butt pad.
    profile("Stock", [
        (0.05, 0.17), (0.05, 0.12), (-0.12, 0.12), (-0.16, 0.06), (-0.18, -0.02), (-0.2, -0.2),
        (-0.31, -0.2), (-0.3, -0.04), (-0.34, 0.08), (-0.78, 0.0), (-0.8, 0.04), (-0.8, 0.27),
        (-0.76, 0.29), (-0.42, 0.25), (-0.3, 0.2),
    ], 0.09, p.frame, bevel=0.012, segments=3)
    for side in (-1, 1):
        profile(f"Grip Texture Panel {side}", [(-0.17, 0.04), (-0.19, -0.17), (-0.29, -0.17), (-0.28, 0.04)],
                0.006, p.texture, x=side * 0.046, bevel=0.002, segments=1)
    box("Cheek Rest", (0.0, -0.58, 0.27), (0.07, 0.24, 0.05), p.accent, bevel=0.008)
    box("Butt Pad", (0.0, -0.81, 0.13), (0.09, 0.02, 0.28), p.rubber, bevel=0.008)

    # Flight rail running forward to the riser, with a bolt groove.
    box("Flight Rail", (0.0, 0.38, rail_z), (0.06, 0.7, 0.05), p.metal, bevel=0.006)
    box("Bolt Groove", (0.0, 0.38, rail_z + 0.026), (0.012, 0.66, 0.004), p.dark, bevel=0.0)
    box("Rail Underside", (0.0, 0.33, rail_z - 0.05), (0.05, 0.5, 0.05), p.frame, bevel=0.008)
    for side in (-1, 1):
        ribs(f"Rail Lightening Cut {side}", 0.15, 0.6, 5, side * 0.031, rail_z, (0.004, 0.05, 0.022), p.dark)

    # Riser and swept-back limbs with cams at the tips.
    box("Riser", (0.0, 0.74, rail_z), (0.14, 0.06, 0.08), p.metal, bevel=0.01, segments=2)
    box("Foot Stirrup Bar", (0.0, 0.83, rail_z - 0.02), (0.12, 0.02, 0.02), p.metal, bevel=0.004)
    for side in (-1, 1):
        flat_bar(f"Foot Stirrup Arm {side}", (side * 0.06, 0.76), (side * 0.06, 0.84), 0.015, 0.015, p.metal, rail_z - 0.02)
    tips = []
    for side in (-1, 1):
        root = (side * 0.06, 0.74)
        tip = (side * 0.43, 0.63)
        tips.append(tip)
        for layer, dz in (("Upper", 0.022), ("Lower", -0.022)):
            flat_bar(f"Limb {layer} {side}", root, tip, 0.04, 0.012, p.frame, rail_z + dz)
        tube_z(f"Cam {side}", rail_z - 0.03, rail_z + 0.03, 0.03, p.metal, x=tip[0], y=tip[1])
        tube_z(f"Cam Axle {side}", rail_z - 0.04, rail_z + 0.04, 0.008, p.accent, x=tip[0], y=tip[1])

    # Cocked string from each cam back to the latch.
    latch = (0.0, 0.12)
    for side, tip in zip((-1, 1), tips):
        flat_bar(f"String {side}", (tip[0] - side * 0.02, tip[1] - 0.02), latch, 0.006, 0.006, p.dark, rail_z + 0.03)
    box("String Latch", (0.0, 0.1, rail_z + 0.035), (0.05, 0.05, 0.03), p.metal, bevel=0.006)

    # Loaded tranquilizer bolt: shaft, glass dose head, and orange fletching.
    tube("Bolt Shaft", 0.12, 0.8, 0.006, p.metal, z=rail_z + 0.035, vertices=10)
    tube("Bolt Dose Vial", 0.78, 0.86, 0.013, p.glass, z=rail_z + 0.035, vertices=14)
    tube("Bolt Dose", 0.785, 0.84, 0.009, p.accent, z=rail_z + 0.035, vertices=12)
    tube("Bolt Needle", 0.86, 0.9, 0.009, p.metal, z=rail_z + 0.035, radius_end=0.001, vertices=10)
    for i in range(3):
        a = i * 2.0 * math.pi / 3.0
        box(f"Bolt Fletching {i + 1}", (math.sin(a) * 0.012, 0.17, rail_z + 0.035 + math.cos(a) * 0.012),
            (0.003, 0.07, 0.016), p.accent, rotation=(0.0, -math.degrees(a), 0.0), bevel=0.0)

    # Trigger group and a forward grip.
    stroke("Trigger Guard", [(-0.03, 0.12), (-0.02, 0.06), (-0.05, 0.035), (-0.15, 0.035)], 0.018, 0.055, p.metal)
    stroke("Trigger", [(-0.085, 0.12), (-0.08, 0.085), (-0.09, 0.06)], 0.014, 0.024, p.metal)
    box("Safety Slider", (0.033, 0.06, rail_z), (0.012, 0.04, 0.02), p.accent, bevel=0.004)
    profile("Forward Grip", [(0.3, rail_z - 0.07), (0.38, rail_z - 0.07), (0.37, -0.05), (0.36, -0.07), (0.31, -0.07), (0.295, -0.05)],
            0.06, p.frame, bevel=0.014, segments=3)

    # Scope.
    scope_z = rail_z + 0.13
    for i, y in enumerate((0.07, 0.29)):
        box(f"Scope Mount {i + 1}", (0.0, y, rail_z + 0.065), (0.04, 0.03, 0.09), p.metal, bevel=0.004)
        tube(f"Scope Ring {i + 1}", y - 0.015, y + 0.015, 0.032, p.metal, z=scope_z)
    tube("Scope Eyepiece", -0.05, 0.03, 0.036, p.dark, z=scope_z)
    tube("Scope Tube", 0.03, 0.33, 0.026, p.dark, z=scope_z)
    tube("Scope Objective", 0.33, 0.41, 0.026, p.dark, z=scope_z, radius_end=0.04)
    tube("Scope Lens", 0.409, 0.413, 0.034, p.glass, z=scope_z, bevel=0.0)
    export_model("Crossbow")


BUILDERS = {
    "TranquilizerPistol": tranquilizer_pistol,
    "SedativeSMG": sedative_smg,
    "PrecisionRifle": precision_rifle,
    "AssaultRifle": assault_rifle,
    "GrenadeLauncher": grenade_launcher,
    "RocketLauncher": rocket_launcher,
    "Crossbow": crossbow,
}

for model_name, builder in BUILDERS.items():
    if ONLY is None or model_name in ONLY:
        builder()

print(f"Generated weapon models in {os.path.join(OUTPUT_DIR, 'Weapons')}")
