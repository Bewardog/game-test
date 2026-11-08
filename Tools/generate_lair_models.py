import bpy
import math
import os
import sys


OUTPUT_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else os.getcwd()


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)


def material(name, color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.metallic = metallic
    mat.roughness = roughness
    return mat


def finish_object(obj, mat=None, bevel=0.04):
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if mat:
        obj.data.materials.append(mat)
    if bevel > 0:
        modifier = obj.modifiers.new("Edge Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
    return obj


def box(name, location, dimensions, mat, rotation=(0.0, 0.0, 0.0), bevel=0.04):
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    return finish_object(obj, mat, bevel)


def cylinder(name, location, radius, depth, mat, rotation=(0.0, 0.0, 0.0), vertices=16):
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


def export_model(name):
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(OUTPUT_DIR, f"{name}.fbx"),
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH"},
        bake_anim=False,
        add_leaf_bones=False,
        path_mode="AUTO",
    )


def research_console():
    clear_scene()
    dark = material("Console Dark", (0.035, 0.06, 0.075), 0.6, 0.3)
    metal = material("Console Metal", (0.18, 0.24, 0.27), 0.75, 0.25)
    screen = material("Console Screen", (0.05, 0.8, 0.85), 0.15, 0.18)
    box("Console Base", (0, 0, 0.48), (2.25, 0.9, 0.95), dark, bevel=0.1)
    box("Console Top", (0, -0.02, 1.05), (2.1, 0.82, 0.18), metal, rotation=(math.radians(-10), 0, 0))
    box("Main Screen", (0, 0.18, 1.55), (1.55, 0.12, 0.78), screen, rotation=(math.radians(14), 0, 0), bevel=0.06)
    box("Left Controls", (-0.78, -0.38, 1.12), (0.38, 0.32, 0.08), screen)
    box("Right Controls", (0.78, -0.38, 1.12), (0.38, 0.32, 0.08), screen)
    export_model("ResearchConsole")


def armory_rack():
    clear_scene()
    metal = material("Rack Metal", (0.12, 0.14, 0.16), 0.8, 0.3)
    accent = material("Rack Accent", (0.7, 0.08, 0.06), 0.45, 0.25)
    weapon = material("Display Weapon", (0.24, 0.28, 0.3), 0.75, 0.2)
    box("Back Panel", (0, 0.18, 1.35), (2.8, 0.18, 2.7), metal, bevel=0.06)
    box("Top Beam", (0, 0, 2.72), (3.0, 0.35, 0.22), accent)
    box("Left Post", (-1.38, 0, 1.35), (0.22, 0.38, 2.7), accent)
    box("Right Post", (1.38, 0, 1.35), (0.22, 0.38, 2.7), accent)
    for index, z in enumerate((0.65, 1.35, 2.05)):
        box(f"Weapon {index} Body", (0, -0.05, z), (1.55, 0.18, 0.2), weapon, rotation=(0, math.radians(5 * (index - 1)), 0))
        box(f"Weapon {index} Grip", (0.35, -0.05, z - 0.25), (0.22, 0.18, 0.48), weapon, rotation=(0, math.radians(12), math.radians(-18)))
        cylinder(f"Weapon {index} Barrel", (-0.95, -0.05, z), 0.07, 0.7, weapon, rotation=(0, math.radians(90), 0), vertices=12)
    export_model("ArmoryRack")


def training_rig():
    clear_scene()
    metal = material("Training Frame", (0.12, 0.17, 0.19), 0.8, 0.3)
    pad = material("Training Pad", (0.75, 0.14, 0.09), 0.1, 0.7)
    target = material("Target", (0.82, 0.68, 0.18), 0.2, 0.5)
    box("Left Frame", (-1.15, 0, 1.5), (0.18, 0.22, 3.0), metal)
    box("Right Frame", (1.15, 0, 1.5), (0.18, 0.22, 3.0), metal)
    box("Top Frame", (0, 0, 2.92), (2.5, 0.22, 0.2), metal)
    cylinder("Punching Bag", (-0.55, 0, 1.65), 0.32, 1.55, pad, vertices=18)
    cylinder("Target Torso", (0.62, 0, 1.45), 0.34, 1.0, target, vertices=12)
    cylinder("Target Head", (0.62, 0, 2.13), 0.24, 0.4, target, vertices=12)
    box("Target Stand", (0.62, 0, 0.45), (0.15, 0.15, 0.9), metal)
    box("Floor Brace", (0.62, 0, 0.08), (1.2, 0.8, 0.16), metal)
    export_model("TrainingRig")


def medical_pod():
    clear_scene()
    shell = material("Medical Shell", (0.68, 0.75, 0.77), 0.35, 0.25)
    dark = material("Medical Base", (0.08, 0.12, 0.14), 0.6, 0.3)
    light = material("Medical Light", (0.12, 0.9, 0.7), 0.1, 0.15)
    box("Pod Base", (0, 0, 0.32), (1.4, 2.8, 0.62), dark, bevel=0.14)
    box("Bed", (0, 0, 0.72), (1.15, 2.45, 0.22), shell, bevel=0.09)
    box("Head Cushion", (0, 0.82, 0.94), (0.82, 0.65, 0.18), shell, rotation=(math.radians(-15), 0, 0), bevel=0.08)
    box("Left Arch", (-0.72, 0.55, 1.55), (0.12, 0.18, 1.9), shell)
    box("Right Arch", (0.72, 0.55, 1.55), (0.12, 0.18, 1.9), shell)
    box("Scanner", (0, 0.55, 2.45), (1.55, 0.35, 0.18), light)
    box("Monitor", (0.9, 0.45, 1.45), (0.12, 0.75, 0.65), light, rotation=(0, math.radians(12), 0))
    export_model("MedicalPod")


def customization_station():
    clear_scene()
    frame = material("Wardrobe Frame", (0.22, 0.12, 0.24), 0.5, 0.3)
    glass = material("Mirror", (0.35, 0.68, 0.78), 0.65, 0.12)
    accent = material("Wardrobe Light", (0.9, 0.2, 0.62), 0.15, 0.15)
    cylinder("Turntable", (0, 0, 0.14), 1.1, 0.28, frame, vertices=32)
    box("Mirror", (0, 0.82, 1.55), (1.7, 0.12, 2.7), glass, bevel=0.1)
    box("Left Column", (-1.05, 0.72, 1.5), (0.18, 0.25, 3.0), frame)
    box("Right Column", (1.05, 0.72, 1.5), (0.18, 0.25, 3.0), frame)
    box("Top Light", (0, 0.7, 2.95), (2.25, 0.28, 0.18), accent)
    box("Control Plinth", (1.35, -0.25, 0.65), (0.55, 0.7, 1.3), frame, bevel=0.08)
    box("Control Screen", (1.35, -0.42, 1.35), (0.42, 0.08, 0.42), accent, rotation=(math.radians(-12), 0, 0))
    export_model("CustomizationStation")


def recruit_bunk():
    clear_scene()
    frame = material("Bunk Frame", (0.12, 0.16, 0.18), 0.7, 0.3)
    fabric = material("Bunk Fabric", (0.16, 0.42, 0.48), 0.0, 0.8)
    locker = material("Locker", (0.25, 0.28, 0.3), 0.6, 0.35)
    box("Bed Frame", (0, 0, 0.35), (1.15, 2.2, 0.38), frame)
    box("Mattress", (0, 0, 0.62), (1.05, 2.05, 0.2), fabric, bevel=0.09)
    box("Pillow", (0, 0.72, 0.79), (0.72, 0.42, 0.16), fabric, bevel=0.08)
    box("Locker", (0.9, 0.5, 1.15), (0.55, 0.75, 2.3), locker, bevel=0.05)
    box("Locker Handle", (0.59, 0.5, 1.2), (0.04, 0.12, 0.28), frame)
    export_model("RecruitBunk")


def intel_console():
    clear_scene()
    dark = material("Intel Base", (0.04, 0.08, 0.1), 0.7, 0.25)
    screen = material("Intel Screens", (0.1, 0.85, 0.45), 0.15, 0.18)
    box("Desk", (0, 0, 0.62), (2.8, 1.05, 1.0), dark, bevel=0.1)
    for index, x in enumerate((-0.9, 0.0, 0.9)):
        box(f"Screen {index}", (x, 0.28, 1.65), (0.78, 0.1, 0.72), screen, rotation=(math.radians(8), 0, 0), bevel=0.06)
    box("Map Table", (0, -0.55, 1.15), (2.35, 0.65, 0.12), screen, rotation=(math.radians(-8), 0, 0))
    export_model("IntelConsole")


def hangar_mech():
    clear_scene()
    armor = material("Mech Armor", (0.2, 0.27, 0.3), 0.8, 0.24)
    dark = material("Mech Joints", (0.035, 0.05, 0.06), 0.9, 0.18)
    glass = material("Mech Cockpit", (0.08, 0.7, 0.82), 0.6, 0.12)
    box("Torso", (0, 0, 2.65), (2.3, 1.2, 1.7), armor, bevel=0.16)
    box("Cockpit", (0, -0.58, 3.15), (1.35, 0.25, 0.78), glass, rotation=(math.radians(-8), 0, 0), bevel=0.1)
    for side in (-1, 1):
        x = 0.72 * side
        cylinder(f"Hip {side}", (x, 0, 1.85), 0.28, 0.5, dark, rotation=(0, math.radians(90), 0))
        box(f"Upper Leg {side}", (x, 0, 1.35), (0.55, 0.72, 1.15), armor, rotation=(0, 0, math.radians(-6 * side)), bevel=0.1)
        cylinder(f"Knee {side}", (x + 0.08 * side, 0, 0.78), 0.24, 0.5, dark, rotation=(0, math.radians(90), 0))
        box(f"Lower Leg {side}", (x + 0.12 * side, 0, 0.38), (0.5, 0.65, 0.85), armor, bevel=0.08)
        box(f"Foot {side}", (x + 0.12 * side, -0.22, 0.08), (0.7, 1.0, 0.22), dark, bevel=0.06)
        box(f"Shoulder {side}", (1.35 * side, 0, 2.95), (0.75, 1.0, 0.8), armor, bevel=0.12)
        box(f"Arm {side}", (1.55 * side, 0, 2.1), (0.45, 0.55, 1.2), armor, rotation=(0, 0, math.radians(8 * side)), bevel=0.08)
    cylinder("Sensor", (0, -0.72, 2.45), 0.16, 0.25, glass, rotation=(math.radians(90), 0, 0), vertices=16)
    export_model("HangarMech")


for builder in (
    research_console,
    armory_rack,
    training_rig,
    medical_pod,
    customization_station,
    recruit_bunk,
    intel_console,
    hangar_mech,
):
    builder()

print(f"Generated lair model kit in {OUTPUT_DIR}")
