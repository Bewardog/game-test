import bpy
import math
import os
import sys


OUTPUT_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else os.getcwd()


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for material_data in list(bpy.data.materials):
        bpy.data.materials.remove(material_data)


def material(name, color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.metallic = metallic
    mat.roughness = roughness
    return mat


def finish_object(obj, mat=None, bevel=0.035):
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if mat:
        obj.data.materials.append(mat)
    if bevel > 0:
        modifier = obj.modifiers.new("Edge Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
    return obj


def box(name, location, dimensions, mat, rotation=(0.0, 0.0, 0.0), bevel=0.035):
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    return finish_object(obj, mat, bevel)


def cylinder(name, location, radius, depth, mat, rotation=(0.0, 0.0, 0.0), vertices=20):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices,
        radius=radius,
        depth=depth,
        location=location,
        rotation=rotation,
    )
    obj = bpy.context.object
    obj.name = name
    return finish_object(obj, mat, 0.025)


def export_model(folder, name):
    output_folder = os.path.join(OUTPUT_DIR, folder)
    os.makedirs(output_folder, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(output_folder, f"{name}.fbx"),
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH"},
        bake_anim=False,
        add_leaf_bones=False,
        path_mode="AUTO",
    )


def server_rack():
    clear_scene()
    shell = material("Rack Shell", (0.055, 0.075, 0.085), 0.8, 0.24)
    frame = material("Rack Frame", (0.17, 0.21, 0.22), 0.75, 0.28)
    panel = material("Rack Panels", (0.08, 0.12, 0.14), 0.45, 0.32)
    status = material("Status Lights", (0.05, 0.82, 0.72), 0.15, 0.14)
    box("Cabinet", (0, 0, 1.1), (0.9, 0.72, 2.2), shell, bevel=0.08)
    box("Front Frame", (0, -0.39, 1.1), (0.82, 0.08, 2.05), frame, bevel=0.03)
    for index in range(7):
        z = 0.35 + index * 0.25
        box(f"Server Drawer {index + 1}", (0, -0.45, z), (0.68, 0.08, 0.16), panel, bevel=0.02)
        box(f"Status {index + 1}", (0.25, -0.5, z), (0.12, 0.025, 0.035), status, bevel=0.01)
    box("Top Vent", (0, 0, 2.22), (0.74, 0.55, 0.08), frame)
    export_model("Facility", "ServerRack")


def security_checkpoint():
    clear_scene()
    shell = material("Checkpoint Shell", (0.08, 0.11, 0.13), 0.65, 0.28)
    armor = material("Checkpoint Armor", (0.28, 0.32, 0.34), 0.75, 0.23)
    screen = material("Checkpoint Screen", (0.08, 0.72, 0.78), 0.2, 0.14)
    warning = material("Checkpoint Warning", (0.9, 0.18, 0.08), 0.25, 0.2)
    box("Desk", (0, 0, 0.62), (2.35, 0.9, 1.05), shell, bevel=0.11)
    box("Armored Front", (0, -0.48, 0.75), (2.18, 0.08, 0.72), armor, bevel=0.04)
    box("Console", (0, -0.18, 1.2), (1.45, 0.54, 0.13), armor, rotation=(math.radians(-13), 0, 0), bevel=0.05)
    box("Main Screen", (-0.42, -0.42, 1.32), (0.55, 0.05, 0.34), screen, rotation=(math.radians(-13), 0, 0), bevel=0.03)
    box("Alert Panel", (0.48, -0.42, 1.32), (0.34, 0.05, 0.34), warning, rotation=(math.radians(-13), 0, 0), bevel=0.03)
    cylinder("Scanner Post", (1.35, 0, 1.05), 0.09, 2.1, armor, vertices=14)
    box("Scanner Head", (1.35, 0, 2.08), (0.44, 0.3, 0.18), screen, bevel=0.05)
    export_model("Facility", "SecurityCheckpoint")


def fabrication_bench():
    clear_scene()
    frame = material("Bench Frame", (0.11, 0.14, 0.15), 0.8, 0.28)
    surface = material("Bench Surface", (0.31, 0.35, 0.36), 0.68, 0.25)
    tool = material("Fabrication Tools", (0.9, 0.42, 0.08), 0.45, 0.24)
    screen = material("Fabrication Screen", (0.12, 0.74, 0.66), 0.18, 0.16)
    box("Work Surface", (0, 0, 0.9), (2.4, 1.0, 0.18), surface, bevel=0.07)
    for x in (-1.0, 1.0):
        box(f"Leg {x}", (x, 0, 0.45), (0.16, 0.78, 0.9), frame)
    box("Back Panel", (0, 0.46, 1.55), (2.35, 0.1, 1.2), frame, bevel=0.05)
    box("Screen", (-0.62, 0.37, 1.62), (0.78, 0.06, 0.52), screen, rotation=(math.radians(6), 0, 0), bevel=0.04)
    cylinder("Tool Spindle", (0.45, -0.08, 1.25), 0.17, 0.62, tool, vertices=16)
    box("Tool Head", (0.45, -0.08, 1.62), (0.42, 0.42, 0.18), tool, bevel=0.05)
    box("Parts Tray", (0.85, -0.2, 1.05), (0.72, 0.48, 0.08), tool, bevel=0.03)
    export_model("Facility", "FabricationBench")


def generator():
    clear_scene()
    shell = material("Generator Armor", (0.19, 0.23, 0.24), 0.78, 0.25)
    dark = material("Generator Dark", (0.045, 0.06, 0.065), 0.88, 0.2)
    hazard = material("Generator Hazard", (0.92, 0.58, 0.06), 0.25, 0.3)
    energy = material("Generator Energy", (0.05, 0.78, 0.72), 0.15, 0.12)
    box("Generator Body", (0, 0, 0.78), (2.4, 1.1, 1.45), shell, bevel=0.14)
    box("Generator Base", (0, 0, 0.13), (2.65, 1.3, 0.25), dark, bevel=0.06)
    for x in (-0.72, 0.0, 0.72):
        box(f"Vent {x}", (x, -0.58, 0.82), (0.5, 0.06, 0.72), dark, bevel=0.02)
    box("Energy Core", (0, 0.57, 0.9), (1.3, 0.08, 0.34), energy, bevel=0.04)
    box("Hazard Stripe Top", (0, -0.59, 1.35), (2.05, 0.04, 0.12), hazard)
    cylinder("Exhaust Left", (-0.72, 0.1, 1.68), 0.13, 0.65, dark, vertices=14)
    cylinder("Exhaust Right", (0.72, 0.1, 1.68), 0.13, 0.65, dark, vertices=14)
    export_model("Facility", "Generator")


def laboratory_tank():
    clear_scene()
    frame = material("Tank Frame", (0.18, 0.22, 0.23), 0.78, 0.22)
    glass = material("Tank Glass", (0.12, 0.62, 0.68), 0.48, 0.12)
    fluid = material("Tank Fluid", (0.06, 0.85, 0.56), 0.08, 0.16)
    dark = material("Tank Base", (0.045, 0.06, 0.07), 0.82, 0.22)
    cylinder("Base", (0, 0, 0.22), 0.72, 0.44, dark, vertices=24)
    cylinder("Tank Vessel", (0, 0, 1.48), 0.58, 2.35, glass, vertices=28)
    cylinder("Fluid Column", (0, 0, 1.38), 0.46, 1.95, fluid, vertices=28)
    cylinder("Top", (0, 0, 2.72), 0.72, 0.36, dark, vertices=24)
    for z in (0.55, 1.45, 2.35):
        cylinder(f"Brace {z}", (0, 0, z), 0.66, 0.12, frame, vertices=24)
    cylinder("Pipe", (0.72, 0, 1.25), 0.08, 1.75, frame, vertices=12)
    box("Status Panel", (0, -0.65, 1.5), (0.55, 0.1, 0.72), fluid, bevel=0.04)
    export_model("Facility", "LaboratoryTank")


def cargo_crate():
    clear_scene()
    shell = material("Crate Shell", (0.19, 0.25, 0.27), 0.62, 0.3)
    frame = material("Crate Frame", (0.055, 0.075, 0.08), 0.82, 0.22)
    label = material("Crate Label", (0.12, 0.76, 0.7), 0.18, 0.22)
    box("Crate Body", (0, 0, 0.6), (1.25, 1.05, 1.2), shell, bevel=0.09)
    for x in (-0.58, 0.58):
        box(f"Vertical Brace {x}", (x, 0, 0.6), (0.1, 1.16, 1.28), frame, bevel=0.02)
    for z in (0.08, 1.12):
        box(f"Horizontal Brace {z}", (0, 0, z), (1.34, 1.16, 0.1), frame, bevel=0.02)
    box("Cargo Label", (0, -0.56, 0.68), (0.62, 0.04, 0.32), label, bevel=0.02)
    export_model("Facility", "CargoCrate")


def arsenal_display():
    clear_scene()
    frame = material("Display Frame", (0.055, 0.075, 0.085), 0.82, 0.2)
    panel = material("Display Panel", (0.18, 0.23, 0.25), 0.68, 0.25)
    weapon = material("Prototype Weapon", (0.35, 0.39, 0.4), 0.85, 0.16)
    light = material("Display Light", (0.08, 0.82, 0.72), 0.15, 0.1)
    box("Display Base", (0, 0, 0.18), (2.7, 0.8, 0.36), frame, bevel=0.08)
    box("Display Back", (0, 0.3, 1.45), (2.55, 0.14, 2.35), panel, bevel=0.08)
    box("Light Strip", (0, 0.2, 2.58), (2.2, 0.08, 0.12), light, bevel=0.03)
    for index, z in enumerate((0.75, 1.45, 2.1)):
        box(f"Weapon Body {index}", (0, 0.12, z), (1.45, 0.16, 0.18), weapon, bevel=0.04)
        cylinder(
            f"Weapon Barrel {index}",
            (-0.92, 0.12, z),
            0.055,
            0.55,
            weapon,
            rotation=(0, math.radians(90), 0),
            vertices=12,
        )
        box(
            f"Weapon Grip {index}",
            (0.35, 0.12, z - 0.24),
            (0.2, 0.16, 0.48),
            weapon,
            rotation=(0, math.radians(6), math.radians(-16)),
            bevel=0.035,
        )
    export_model("Facility", "ArsenalDisplay")


def facility_lamp_post():
    clear_scene()
    metal = material("Lamp Metal", (0.08, 0.11, 0.12), 0.82, 0.2)
    shell = material("Lamp Shell", (0.25, 0.3, 0.31), 0.72, 0.23)
    light = material("Lamp Light", (0.68, 0.96, 0.92), 0.1, 0.08)
    cylinder("Base", (0, 0, 0.12), 0.36, 0.24, shell, vertices=20)
    cylinder("Post", (0, 0, 2.35), 0.1, 4.5, metal, vertices=14)
    box("Cross Arm", (0.38, 0, 4.5), (0.85, 0.14, 0.14), metal, bevel=0.04)
    box("Lamp Housing", (0.78, 0, 4.43), (0.65, 0.42, 0.18), shell, bevel=0.07)
    box("Lamp Lens", (0.78, 0, 4.31), (0.5, 0.32, 0.06), light, bevel=0.03)
    export_model("Facility", "FacilityLampPost")


def weapon_model(name, body_color, accent_color, barrel_length, body_length, stock_length, magazine_depth):
    clear_scene()
    body = material(f"{name} Body", body_color, 0.82, 0.18)
    dark = material(f"{name} Dark", (0.025, 0.035, 0.04), 0.9, 0.16)
    accent = material(f"{name} Accent", accent_color, 0.3, 0.16)

    box("Receiver", (0, 0.18, 0.1), (0.28, body_length, 0.25), body, bevel=0.055)
    cylinder(
        "Barrel",
        (0, body_length * 0.5 + barrel_length * 0.5, 0.12),
        0.065,
        barrel_length,
        dark,
        rotation=(math.radians(90), 0, 0),
        vertices=14,
    )
    box(
        "Grip",
        (0, -body_length * 0.18, -0.22),
        (0.2, 0.25, 0.48),
        dark,
        rotation=(math.radians(-12), 0, 0),
        bevel=0.045,
    )
    box("Magazine", (0, 0.12, -0.25), (0.16, 0.22, magazine_depth), accent, bevel=0.035)
    box("Sight", (0, 0.14, 0.28), (0.1, 0.22, 0.1), accent, bevel=0.025)
    if stock_length > 0:
        box("Stock", (0, -body_length * 0.5 - stock_length * 0.5, 0.02), (0.24, stock_length, 0.3), body, bevel=0.055)

    export_model("Weapons", name)


for builder in (
    server_rack,
    security_checkpoint,
    fabrication_bench,
    generator,
    laboratory_tank,
    cargo_crate,
    arsenal_display,
    facility_lamp_post,
):
    builder()

weapon_model("TranquilizerPistol", (0.05, 0.07, 0.08), (0.1, 0.82, 0.72), 0.28, 0.48, 0.0, 0.35)
weapon_model("SedativeSMG", (0.07, 0.09, 0.1), (0.75, 0.16, 0.22), 0.32, 0.72, 0.28, 0.52)
weapon_model("PrecisionRifle", (0.08, 0.1, 0.11), (0.12, 0.56, 0.88), 0.62, 0.92, 0.58, 0.42)
weapon_model("ShockPistol", (0.08, 0.07, 0.1), (0.72, 0.28, 0.92), 0.22, 0.52, 0.0, 0.32)

print(f"Generated facility and weapon model kits in {OUTPUT_DIR}")
