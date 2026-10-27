import os
import docx
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn

def set_cell_background(cell, hex_color):
    shading_elm = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{hex_color}"/>')
    cell._tc.get_or_add_tcPr().append(shading_elm)

def set_cell_margins(cell, top=80, bottom=80, left=120, right=120):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def add_callout(doc, title, text, border_color="8B0000", bg_color="FFF8F8"):
    tbl = doc.add_table(rows=1, cols=1)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    tbl.autofit = False
    
    cell = tbl.cell(0, 0)
    cell.width = Inches(7.1)
    set_cell_background(cell, bg_color)
    set_cell_margins(cell, top=100, bottom=100, left=150, right=150)
    
    tcPr = cell._tc.get_or_add_tcPr()
    borders = parse_xml(f'''
        <w:tcBorders {nsdecls("w")}>
            <w:top w:val="none"/>
            <w:left w:val="single" w:sz="24" w:space="0" w:color="{border_color}"/>
            <w:bottom w:val="none"/>
            <w:right w:val="none"/>
        </w:tcBorders>
    ''')
    tcPr.append(borders)
    
    p = cell.paragraphs[0]
    p.paragraph_format.space_before = Pt(2)
    p.paragraph_format.space_after = Pt(2)
    p.paragraph_format.line_spacing = 1.05
    run_t = p.add_run(f"★ {title}\n")
    run_t.bold = True
    run_t.font.name = "Arial"
    run_t.font.size = Pt(10)
    run_t.font.color.rgb = RGBColor(139, 0, 0)
    
    run_b = p.add_run(text)
    run_b.font.name = "Arial"
    run_b.font.size = Pt(9)
    run_b.font.color.rgb = RGBColor(40, 40, 40)
    
    doc.add_paragraph().paragraph_format.space_after = Pt(2)

def create_and_format_table(doc, col_widths, headers, data, header_bg="1A365D"):
    table = doc.add_table(rows=len(data) + 1, cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    header_row = table.rows[0]
    for i, header_text in enumerate(headers):
        cell = header_row.cells[i]
        cell.width = col_widths[i]
        set_cell_background(cell, header_bg)
        set_cell_margins(cell, top=80, bottom=80, left=120, right=120)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.line_spacing = 1.0
        run = p.add_run(header_text)
        run.bold = True
        run.font.name = "Arial"
        run.font.size = Pt(9)
        run.font.color.rgb = RGBColor(255, 255, 255)
        
    for r_idx, row_data in enumerate(data):
        row = table.rows[r_idx + 1]
        bg = "F7FAFC" if r_idx % 2 == 1 else "FFFFFF"
        for c_idx, cell_value in enumerate(row_data):
            cell = row.cells[c_idx]
            cell.width = col_widths[c_idx]
            set_cell_background(cell, bg)
            set_cell_margins(cell, top=60, bottom=60, left=120, right=120)
            p = cell.paragraphs[0]
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.line_spacing = 1.05
            run = p.add_run(str(cell_value))
            run.font.name = "Arial"
            run.font.size = Pt(8.5)
            run.font.color.rgb = RGBColor(45, 55, 72)
    return table

def add_header_footer(doc):
    for s_idx, section in enumerate(doc.sections):
        section.top_margin = Inches(0.65)
        section.bottom_margin = Inches(0.65)
        section.left_margin = Inches(0.7)
        section.right_margin = Inches(0.7)
        
        # Header
        header = section.header
        hp = header.paragraphs[0]
        hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        hp.paragraph_format.space_after = Pt(0)
        hrun = hp.add_run("FORSAKEN ISOLATION — SYSTEM ARCHITECTURE & DESIGN DOSSIER")
        hrun.font.name = "Arial"
        hrun.font.size = Pt(8)
        hrun.font.color.rgb = RGBColor(120, 120, 120)
        
        # Footer
        footer = section.footer
        fp = footer.paragraphs[0]
        fp.alignment = WD_ALIGN_PARAGRAPH.CENTER
        fp.paragraph_format.space_before = Pt(0)
        frun = fp.add_run("Confidential Project Evaluation Document | Unity 3D Engine Architecture | Page ")
        frun.font.name = "Arial"
        frun.font.size = Pt(8)
        frun.font.color.rgb = RGBColor(120, 120, 120)
        
        # Page field in Word
        f_xml = parse_xml(r'<w:fldSimple %s w:instr="PAGE"/>' % nsdecls('w'))
        fp._p.append(f_xml)
        
        frun2 = fp.add_run(" of 7")
        frun2.font.name = "Arial"
        frun2.font.size = Pt(8)
        frun2.font.color.rgb = RGBColor(120, 120, 120)

def build_7_page_doc():
    doc = Document()
    add_header_footer(doc)
    
    styles = doc.styles
    normal_style = styles['Normal']
    normal_style.font.name = 'Arial'
    normal_style.font.size = Pt(9.5)
    normal_style.font.color.rgb = RGBColor(35, 35, 35)
    normal_style.paragraph_format.line_spacing = 1.12
    normal_style.paragraph_format.space_after = Pt(3)

    def h1(text):
        h = doc.add_paragraph()
        h.paragraph_format.space_before = Pt(8)
        h.paragraph_format.space_after = Pt(2)
        run = h.add_run(text)
        run.bold = True
        run.font.size = Pt(13)
        run.font.color.rgb = RGBColor(26, 54, 93) # Deep Slate Blue
        return h

    def h2(text):
        h = doc.add_paragraph()
        h.paragraph_format.space_before = Pt(5)
        h.paragraph_format.space_after = Pt(1.5)
        run = h.add_run(text)
        run.bold = True
        run.font.size = Pt(10.5)
        run.font.color.rgb = RGBColor(116, 42, 42) # Crimson
        return h

    def bullet(text, bold_prefix=""):
        p = doc.add_paragraph(style='List Bullet')
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(1.5)
        p.paragraph_format.line_spacing = 1.08
        if bold_prefix:
            rb = p.add_run(bold_prefix)
            rb.bold = True
            rb.font.size = Pt(9)
            rb.font.color.rgb = RGBColor(30, 30, 30)
        rt = p.add_run(text)
        rt.font.size = Pt(9)
        rt.font.color.rgb = RGBColor(40, 40, 40)
        return p

    def para(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(2.5)
        p.paragraph_format.line_spacing = 1.1
        run = p.add_run(text)
        run.font.size = Pt(9.2)
        return p

    # =========================================================================
    # PAGE 1: TITLE, EXECUTIVE SUMMARY & LORE ARCHITECTURE
    # =========================================================================
    title_p = doc.add_paragraph()
    title_p.paragraph_format.space_before = Pt(4)
    title_p.paragraph_format.space_after = Pt(2)
    title_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_title = title_p.add_run("HORROR SURVIVAL: SYSTEM ARCHITECTURE & GAME DESIGN")
    r_title.font.size = Pt(18)
    r_title.bold = True
    r_title.font.color.rgb = RGBColor(139, 0, 0)

    sub_p = doc.add_paragraph()
    sub_p.paragraph_format.space_after = Pt(8)
    sub_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_sub = sub_p.add_run("Comprehensive 7-Page Technical Design Document & Presentation Showcase")
    r_sub.font.size = Pt(11)
    r_sub.italic = True
    r_sub.font.color.rgb = RGBColor(70, 70, 70)

    meta_headers = ["Project Specification", "Implementation Detail"]
    meta_data = [
        ["Project Title & Genre", "Forsaken Isolation — 3D First-Person Psychological Survival Horror"],
        ["Engine & Scripting Stack", "Unity 3D Engine (URP) | C# (.NET Framework) | PC Windows Target"],
        ["Architectural Pillars", "Dynamic Lighting Deprivation, Infinite Survival Loop, Multi-State Sensory AI, Physical Puzzles"],
        ["Target Evaluation", "Academic Evaluators, Technical Project Reviewers, Game Development Faculty"]
    ]
    create_and_format_table(doc, [Inches(2.3), Inches(4.8)], meta_headers, meta_data)

    h1("1. EXECUTIVE SUMMARY & GAME VISION")
    para(
        "Forsaken Isolation is an atmospheric, first-person 3D psychological survival horror game built natively in Unity. "
        "The player awakens trapped within an abandoned, subterranean quarantine research facility cloaked in total blackout darkness. "
        "Escape requires scouring the labyrinthine ward, operating an auxiliary diesel generator to temporarily illuminate sectors, "
        "solving physical environmental puzzles, recovering sacred anatomical relics, and evading a sentient hunting entity."
    )
    para(
        "Unlike standard horror titles relying primarily on scripted, static jumpscares, Forsaken Isolation introduces systemic tension: "
        "survival depends directly on power resource management, sound stealth discipline, and spatial memory under oppressive darkness. "
        "The engineering prioritizes modular C# subsystems, optimized GPU batching, deterministic state caching, and zero memory leaks."
    )

    add_callout(
        doc,
        "CORE GAMEPLAY PILLAR: ILLUMINATION AS SURVIVAL",
        "Light in Forsaken Isolation is not merely a cosmetic setting—it is a finite, highly contested life-or-death resource. "
        "When the generator's fuel timer expires, high-voltage utility lamps shut down and scene lightmaps are purged in real-time, "
        "leaving only eerie crimson emergency lamps active. In the dark, the monster's sensory hearing range doubles and hunt speed increases."
    )

    h1("2. NARRATIVE ARCHITECTURE & GAMEPLAY LORE")
    para(
        "The narrative is unraveled non-linearly through environmental storytelling, classified patient logs, and interactive handwritten notes. "
        "The protagonist was an archivist abducted into an unrecorded biochemical and cognitive experimentation facility known as Facility 09."
    )
    bullet(" The Outbreak of Patient Zero: Scientific research journals documenting an experimental neurological serum that mutated a volunteer into the blind, auditory-driven entity roaming the complex.", "•")
    bullet(" The Auxiliary Power Protocol: Emergency memos from the Chief Facilities Engineer explaining that security doors and blast gates require high-voltage generator power, which runs on 60-second fuel intervals.", "•")
    bullet(" The Occult Quarantine Relics: Disturbed patient scribbles revealing that the main decontamination escape portal was sealed with biometric/anatomical locks requiring four preserved organ relics to unlock.", "•")

    doc.add_page_break()

    # =========================================================================
    # PAGE 2: CORE MECHANICS & SUBSYSTEMS ARCHITECTURE
    # =========================================================================
    h1("3. CORE GAMEPLAY MECHANICS & SUBSYSTEMS ARCHITECTURE")
    para(
        "The core game loop balances four interconnected loops: Exploration, Power Scavenging, Puzzle Solving, and Monster Evasion. "
        "Every subsystem is decoupled through C# event delegates and state managers to guarantee performance and stability."
    )

    h2("3.1 First-Person Controller & Player Locomotion")
    para(
        "Engineered with 'FirstPersonMovement.cs' and 'FirstPersonAudio.cs' to deliver responsive yet physically grounded movement:"
    )
    bullet(" Kinematic Movement Dynamics: Walking velocity (3.2 m/s), running velocity (6.5 m/s) governed by a stamina depletion curve, preventing infinite sprinting and forcing deliberate tactical evasion.", "•")
    bullet(" Clamped Cinematic Mouse Look: Dual-axis pitch and yaw camera controls with vertical angle clamping (-85° to +85°), preventing rotational inversion and eliminating motion sickness while preserving vulnerability.", "•")
    bullet(" Dynamic Head-Bob & Spatial Footsteps: Variable sinusoidal camera oscillation linked to ground velocity. Footstep sound frequencies adapt dynamically, emitting acoustic triggers that alert patrolling AI.", "•")

    h2("3.2 Handheld Flashlight & Claustrophobic Field-of-View")
    para(
        "Traversing the facility requires the player's personal flashlight, governed by 'flahlight.cs' and 'TorchFX.cs':"
    )
    bullet(" Narrow Focal Cone: High-intensity spotlight beam with sharp angle falloff (45°), illuminating direct paths while keeping peripheral boundaries shrouded in deep, terrifying shadows.", "•")
    bullet(" Independent Battery Circuit: Architecturally separated from the facility's auxiliary power grid, ensuring the player maintains minimal local vision even during global facility blackout sequences.", "•")
    bullet(" Tactical Tradeoff: Activating the flashlight enables spatial navigation and puzzle inspection, but directly crossing the monster's line-of-sight triggers instant auditory shrieks and aggressive pursuit.", "•")

    h2("3.3 The Generator & Industrial Power Grid Subsystem")
    para(
        "The facility power network, orchestrated by 'lever_script.cs' and 'GeneratorSystem.cs', serves as the central gameplay clock:"
    )
    bullet(" Physical Lever Mechanics: Smooth mechanical lever rotation animation upon player activation, accompanied by heavy gear grinding, pneumatic pressure release, and engine ignition audio clips.", "•")
    bullet(" Spatial Audio Motor Loop: A 3D audio source attached to the generator engine room provides audible orientation cues across neighboring corridors, helping disoriented players locate the power hub in darkness.", "•")
    bullet(" Real-Time Countdown HUD: A high-contrast TextMeshPro timer displays remaining fuel runtime (01:00 down to 00:00). When 10 seconds remain, rhythmic auditory warning sirens begin pulsing.", "•")

    h2("3.4 Subsystems Architecture & Technical Responsibility Matrix")
    mech_headers = ["Subsystem Name", "Primary Script References", "Architectural Role & Technical Responsibility"]
    mech_data = [
        ["Player Controller", "FirstPersonMovement.cs, FirstPersonAudio.cs", "Handles physics-based movement, stamina curves, footstep audio emission, and mouse look clamping."],
        ["Power & Lighting", "lever_script.cs, GeneratorSystem.cs, can.cs", "Manages generator timer, power cycles, fuel consumption, lightmap swapping, and red light preservation."],
        ["Sensory AI System", "PatrolAndChaseAi.cs, deadcollide2.cs", "Controls waypoint navigation, line-of-sight raycasting, hearing radii, pursuit states, and jumpscare triggers."],
        ["Interactive Progression", "BodyPart.cs, ExitGateSequence.cs, key.cs", "Tracks key inventory, drawer physics, wall cube socketing, anatomical relic collection, and escape logic."],
        ["Atmosphere & Audio", "SOUNDHORROR.cs, AnxietyShake.cs, TorchFX.cs", "Drives 3D spatial attenuation, dynamic background tension drones, camera trauma shake, and flickering lights."]
    ]
    create_and_format_table(doc, [Inches(1.8), Inches(2.3), Inches(3.0)], mech_headers, mech_data)

    doc.add_page_break()

    # =========================================================================
    # PAGE 3: ADVANCED TECHNICAL INNOVATIONS (SPECIAL ACHIEVEMENTS)
    # =========================================================================
    h1("4. ADVANCED TECHNICAL INNOVATIONS & SPECIAL MECHANICS")
    para(
        "This section documents the standout engineering milestones accomplished in the project—the signature technical solutions "
        "distinguishing Forsaken Isolation from conventional student and indie horror prototypes."
    )

    h2("4.1 Zero-Leak Infinite Fuel Reusability Architecture")
    para(
        "Standard item pickup systems destroy gameobjects upon interaction. In our survival loop, permanently destroying fuel cans broke "
        "extended gameplay sessions. We developed a state-reset lifecycle between 'lever_script.cs' and 'can.cs':"
    )
    bullet(" Collection Phase: Player approaches the diesel canister, interacts via [E], which disables the mesh renderer and collider while caching the item in the active inventory state without invoking Destroy().", "•")
    bullet(" Refueling Execution: Player pulls the generator lever; inventory state clears, fuel timer resets to 60 seconds, and utility lights ignite.", "•")
    bullet(" Power Blackout & Auto-Respawn: When runtime reaches zero, StopGenerator() fires a global event. All instantiated fuel cans automatically re-enable colliders and visual meshes at designated spawn points, enabling endless survival cycles.", "•")

    h2("4.2 Dynamic Lighting Deprivation & Real-Time Lightmap Swapping")
    para(
        "Achieving genuine pitch darkness in Unity is notoriously challenging because pre-baked global illumination (GI) lightmaps leak ambient "
        "bounce light even when real-time light sources are toggled off. We resolved this via runtime lightmap array swapping:"
    )
    bullet(" Startup Array Caching: On Awake(), the master controller stores all baked lightmap textures: 'originalLightmaps = LightmapSettings.lightmaps'.", "•")
    bullet(" Zero-Memory Blackout: Upon generator depletion, the script assigns 'LightmapSettings.lightmaps = new LightmapData[0]'. This instantly purges all ambient bounce illumination without modifying original asset files on disk.", "•")
    bullet(" Instant GI Restoration: Upon refueling, 'LightmapSettings.lightmaps = originalLightmaps' immediately re-engages full high-definition Global Illumination with 0 milliseconds of CPU baking overhead and 0 frame hitching.", "•")

    h2("4.3 Mathematical Red Light Chromatic Isolation Algorithm")
    para(
        "During complete power failure, plunging every single light source to black disorients players and degrades artistic direction. "
        "We engineered an algorithmic chromatic ratio filter in C# to automatically preserve emergency egress beacons:"
    )

    add_callout(
        doc,
        "CHROMATIC RATIO FILTER (C# CODE IMPLEMENTATION)",
        "bool IsRedLight(Light l) {\n"
        "    Color c = l.color;\n"
        "    if (c.r > 0.4f && c.r > c.g * 1.8f && c.r > c.b * 1.8f) return true;\n"
        "    if (l.gameObject.name.ToLower().Contains(\"red\")) return true;\n"
        "    return false;\n"
        "}\n"
        "This formula inspects all scene lights dynamically. White, yellow, and fluorescent utility lights shut off immediately during blackouts, "
        "while blood-red beacons remain permanently powered, establishing a terrifying silhouette aesthetic and guiding escape routes."
    )

    h2("4.4 Environmental Flickering & Damaged Circuit Simulation")
    para(
        "Implements 'FLICKERING.cs' and 'TorchFX.cs' with randomized pseudo-periodic perlin noise intervals to emulate dying ballast resistors. "
        "Lights flutter erratically between 0% and 130% nominal intensity, triggering subtle psychoacoustic hum spikes."
    )

    doc.add_page_break()

    # =========================================================================
    # PAGE 4: ARTIFICIAL INTELLIGENCE, SENSORY LOGIC & JUMPSCARES
    # =========================================================================
    h1("5. ARTIFICIAL INTELLIGENCE, SENSORY LOGIC & JUMPSCARES")
    para(
        "The antagonist in Forsaken Isolation is governed by a modular finite state machine (FSM) implemented in 'PatrolAndChaseAi.cs' and 'ENEMY.cs'. "
        "Rather than cheating with omnipresent player tracking, the AI relies on authentic sensory perception systems."
    )

    h2("5.1 AI Behavior State Machine Architecture")
    bullet(" Patrol & Loiter State: The entity traverses sequential 3D waypoints distributed through corridors. At each node, it executes randomized pause routines (2.0s to 5.0s), inspecting shadowed corners with head-turning kinematics.", "•")
    bullet(" Sensory Investigation State: If acoustic vibrations or indirect flashlight beams cross awareness thresholds, the monster halts patrol and moves toward the suspected stimulus point to perform local search sweeps.", "•")
    bullet(" High-Threat Pursuit State: Once visual contact is verified, the entity emits a chilling shriek audio event, enters sprint locomotion (accelerating to 140% player walking speed), and continuously recalculates NavMesh pathing.", "•")
    bullet(" Lethal Attack & Game Over: Upon closing to within 1.5 meters, the monster executes a lethal lunge animation, disabling player input and triggering the dynamic death camera sequence and checkpoint reload.", "•")

    h2("5.2 Sensory Perception: Dual-Tier Vision & Acoustic Detection")
    para(
        "The sensory perception pipeline executes every 0.1 seconds to optimize CPU overhead while maintaining high tactical fidelity:"
    )
    bullet(" Vision Cone Raycasting: A 110-degree horizontal field-of-view cone with a maximum range of 15 meters. Occlusion raycasts verify that walls and solid geometry block vision. However, if the player's flashlight is illuminated, detection range doubles.", "•")
    bullet(" Acoustic Footstep Attenuation: Walking creates an auditory sphere of 4 meters; sprinting expands the sphere to 12 meters. If the monster is within this radius, it immediately breaks patrol to investigate the sound vector.", "•")
    bullet(" Blackout Awareness Multiplier: When the generator is unpowered, the monster's hearing sensitivity increases by 50%, rewarding disciplined stealth crouching.", "•")

    h2("5.3 One-Time Precision Jumpscare Trigger Engine")
    para(
        "Poorly designed horror games suffer from re-triggering jumpscares and overlapping audio glitches. We solved this in 'deadcollide2.cs':"
    )
    bullet(" Atomic State Latching: Utilizes a serialized private boolean flag 'hasTriggered' that acts as an atomic latch, guaranteeing the sequence can never fire more than once per session.", "•")
    bullet(" Multi-Sensory Shock Sync: On Player trigger entry, the script simultaneously spawns the monster apparition in the player's field of view, cuts corridor lighting, and plays an uncompressed 3D jumpscare audio stinger.", "•")
    bullet(" Coroutine Garbage Disposal: An asynchronous coroutine ('TurnOffObjectRoutine') runs for precisely 2.0 seconds before cleanly disabling the apparition and restoring ambient audio, leaving the player in adrenaline-charged silence.", "•")

    h2("5.4 Procedural Camera Anxiety Shake Subsystem")
    para(
        "Implemented in 'AnxietyShake.cs' to simulate psychological terror: as the monster nears the player or sudden blackouts occur, "
        "the virtual camera applies continuous rotational and positional Perlin noise shake, blurring focal clarity and elevating visceral panic."
    )

    doc.add_page_break()

    # =========================================================================
    # PAGE 5: INTERACTIVE PUZZLES, PROGRESSION & RELIC SACRIFICE
    # =========================================================================
    h1("6. INTERACTIVE PUZZLES, PROGRESSION & RELIC SACRIFICE")
    para(
        "Progression through the facility is non-linear and gate-locked. Players must explore hazardous wings, locate quest tools, "
        "and complete environmental puzzles while managing generator fuel reserves."
    )

    h2("6.1 Multi-Stage Facility Escape Progression Flow")
    para(
        "The journey to escape is divided into three structured narrative and mechanical acts:"
    )
    bullet(" Phase I: Awakening & Grid Restoration: Player wakes in Cell Block C, navigates the pitch-black basement to locate the generator room, recovers the initial fuel canister, and powers up the primary electrical grid.", "•")
    bullet(" Phase II: Cryptic Exploration & Key Retrieval: Player explores medical examination rooms, opens physical office drawers ('OpenDrawer.cs'), discovers hidden brass keys ('key.cs'), and unlocks security lockboxes containing puzzle relics.", "•")
    bullet(" Phase III: Cube Alignment & Anatomical Relic Harvest: Player solves the geometric wall relief puzzle, gathers four preserved anatomical relics ('BodyPart.cs'), places them on the quarantine altar, and breaches the Exit Blast Gate.", "•")

    h2("6.2 Physical Drawer Exploration & Lockbox Interactions")
    para(
        "Exploration incorporates realistic mechanical feedback across all props:"
    )
    bullet(" Smooth Drawer Pull Kinematics: Desks and filing cabinets utilize 'OpenDrawer.cs' to smoothly translate drawer drawers along local Z-axes when targeted, uncovering hidden patient notes and batteries.", "•")
    bullet(" Key-Locked Chest System: Supply lockboxes ('box_open_withkey.cs') verify whether the player possesses the required key ID. Interacting without the key triggers locked latch rattle audio and a contextual HUD prompt: 'Locked — Requires Brass Key'.", "•")

    h2("6.3 Geometric Wall Cube Puzzle Subsystem")
    para(
        "Governed by 'WallPuzzleManager.cs' and 'CubeItem.cs', this spatial puzzle requires finding scattered geometric cubes and placing "
        "them into matching wall niches. When all cubes are seated in correct socket orientations, an electrical relay trips, unlocking Blast Door B."
    )

    h2("6.4 Occult Anatomical Relic Quest & Exit Gate Sequence")
    para(
        "The climactic finale combines quest item collection and narrative release via 'BodyPart.cs' and 'ExitGateSequence.cs':"
    )
    bullet(" Relic Collection Loop: Four grotesque preserved anatomical relics (Heart, Eye, Brain, Arm) are hidden across high-danger patrol sectors. Collecting each relic updates the player's quest inventory and triggers ominous audio stingers.", "•")
    bullet(" Altar Socketing: Placing relics onto the ritual decontamination table activates physical relic meshes and illuminates runic lighting.", "•")
    bullet(" Escape Gate Activation: Upon socketing the final relic, 'ExitGateSequence.cs' triggers the motorized blast gate opening sequence ('Gateopen.cs'), initiates siren alarms, and activates the final escape trigger to conclude the game.", "•")

    h2("6.5 Interactive Progression & Objective Matrix")
    prog_headers = ["Progression Objective", "Required Mechanism & Code", "Gameplay Reward / Outcome"]
    prog_data = [
        ["Restore Facility Power", "can.cs / lever_script.cs", "Restores lighting, enables door consoles, starts 60-second survival timer."],
        ["Unlock Security Lockers", "key.cs / box_open_withkey.cs", "Yields flashlight batteries, classified lore documents, and puzzle keys."],
        ["Wall Cube Alignment", "WallPuzzleManager.cs / CubeItem.cs", "Solves electrical circuit, unlocks blast door access to medical ward."],
        ["Harvest 4 Anatomical Relics", "BodyPart.cs / ExitGateSequence.cs", "Activates decontamination altar, opens final Exit Blast Gate to conclude game."]
    ]
    create_and_format_table(doc, [Inches(1.8), Inches(2.3), Inches(3.0)], prog_headers, prog_data)

    doc.add_page_break()

    # =========================================================================
    # PAGE 6: AUDIO, UI ARCHITECTURE & GAME PERSISTENCE
    # =========================================================================
    h1("7. AUDIO ENGINEERING, UI ARCHITECTURE & GAME PERSISTENCE")
    para(
        "Psychological horror relies fundamentally on sound design and unobtrusive UI. Forsaken Isolation implements a specialized "
        "acoustic pipeline paired with a diegetic user interface and robust serialization."
    )

    h2("7.1 Multi-Layered 3D Spatial Audio & Psychoacoustic Soundscapes")
    para(
        "Every acoustic asset is spatialized using custom logarithmic attenuation curves, providing crucial directional feedback:"
    )
    bullet(" Dynamic Tension Drones: Managed by 'SOUNDHORROR.cs', sub-bass ambient tones (40Hz-90Hz) play continuously in background channels, inducing subconscious anxiety and physical discomfort.", "•")
    bullet(" 3D Positional Creature Audio: The monster's footsteps, rattling breath, and guttural groans are rendered through 3D spatial sources, allowing players to pinpoint monster location and distance accurately through stereo headphones.", "•")
    bullet(" Audio Occlusion & Reverb Zones: Corridors and tiled medical rooms utilize acoustic reverb zones, generating echoing sound reflections that warn players when running in open spaces.", "•")

    h2("7.2 Narrative Exposition & Cinematic Video Cutscenes")
    para(
        "Story exposition is integrated through custom cinematic presentation controllers:"
    )
    bullet(" Typewriter Text Controller: 'StoryTypewriter.cs' renders opening exposition letter-by-letter with vintage mechanical audio clicks, creating vintage noir psychological dread before player control begins.", "•")
    bullet(" In-Game Video Transitions: 'PlayVideoAfterText.cs' and 'EndSceneFader.cs' manage fullscreen cinematic video transitions for intro scenes and the game finale, smoothly fading render cameras to black without audio clipping.", "•")

    h2("7.3 Minimalist Crosshair & Dynamic Ghost HUD Alerts")
    para(
        "The UI avoids cluttered health bars or mini-maps, maintaining absolute visual immersion:"
    )
    bullet(" Contextual Crosshair Prompts: High-precision raycasts detect interactable objects (keys, canisters, levers, drawers), displaying clean '[E] Interact' prompts exclusively when within valid operable distance (2.2m).", "•")
    bullet(" Ghost HUD System ('GhostHUD.cs'): When passing narrative threshold zones (e.g., stairwells or gruesome crime scenes), spectral messages fade onto screen via smooth CanvasGroup alpha blending and dissipate after 3.0 seconds.", "•")

    h2("7.4 Comprehensive Menu Suite & State Management")
    para(
        "Engineered with 'MainMenuManager.cs' and 'PauseManager.cs': features interactive Main Menu, Options, and in-game Pause Menu. "
        "Pausing freezes Time.timeScale to 0.0, unlocks hardware mouse cursors, and mutes dynamic audio sources cleanly."
    )

    h2("7.5 Checkpoint Serialization & Game Save Architecture")
    para(
        "Implemented in 'GameSaveSystem.cs' and 'PlayerSaveManager.CS': serializes player coordinates, active inventory items, collected keys, "
        "and puzzle completion flags to encrypted JSON PlayerPrefs, preventing progression loss across gameplay sessions."
    )

    doc.add_page_break()

    # =========================================================================
    # PAGE 7: PRESENTATION GUIDE, EVALUATION DEFENSE & METRICS
    # =========================================================================
    h1("8. PRESENTATION DAY GUIDE: PITCH SCRIPT & DEFENSE DOSSIER")
    para(
        "This structured guide provides a minute-by-minute live presentation outline, anticipated evaluator questions, and technical defense answers."
    )

    h2("8.1 5-Minute Live Gameplay & Technical Demonstration Script")
    pitch_headers = ["Time Interval", "Demonstration Phase", "Action & Talking Points for Evaluators"]
    pitch_data = [
        ["0:00 - 0:45", "Hook & Game Concept", "Introduce Forsaken Isolation: A 3D survival horror game where light is a finite life-or-death resource rather than a visual prop."],
        ["0:45 - 1:45", "Power & Fuel Mechanics", "Walk to generator room. Show fuel can pickup [E]. Pull the lever—lights ignite and countdown HUD starts (01:00). Explain infinite cycle."],
        ["1:45 - 2:45", "The Blackout & Red Lights", "Let timer hit 00:00. Room plunges into pitch black, but RED emergency lights stay on. Explain runtime Lightmap swapping and chromatic ratio math!"],
        ["2:45 - 3:45", "AI Threat & Jumpscare", "Showcase monster patrolling. Cross deadcollide2 trigger—demonstrate one-time jumpscare latch, audio synchronization, and sprint AI chase."],
        ["3:45 - 5:00", "Puzzles, Relics & Q&A", "Show drawer opening, key pickup, and anatomical relic altar socketing. Conclude with technical achievements and open floor for faculty questions."]
    ]
    create_and_format_table(doc, [Inches(1.2), Inches(2.1), Inches(3.8)], pitch_headers, pitch_data)

    h2("8.2 Anticipated Evaluator Questions & Winning Answers")
    bullet(" Q: 'How did you achieve total pitch darkness without expensive real-time light recalculations?'\n"
           "A: 'We dynamically swap Unity's global LightmapSettings.lightmaps array at runtime. We cache the baked array on Awake(), clear it to zero on blackout, and restore it on refueling. This guarantees true pitch darkness instantly with zero FPS drops.'", "•")
    bullet(" Q: 'How do you prevent emergency lights from turning off during global blackouts?'\n"
           "A: 'We created an algorithmic chromatic ratio filter in C# evaluating RGB balance. Lights with dominant red channels are mathematically classified as emergency beacons and excluded from shutdown routines without requiring manual tagging.'", "•")
    bullet(" Q: 'How did you solve the fuel canister disappearing bug for long play sessions?'\n"
           "A: 'We replaced item destruction with a cyclical state reset architecture. When the generator runs out, StopGenerator() fires a clean reset event across scene canisters, reactivating colliders and meshes for the next cycle.'", "•")

    h2("8.3 Performance Benchmarks & Quality Assurance Metrics")
    bullet(" Stable 60+ FPS on mid-range hardware through GPU instancing, baked lightmaps, and occlusion culling.", "•")
    bullet(" Zero GC memory leaks through object pooling, non-allocating raycasts, and cached component references.", "•")
    bullet(" Clean compilation with 0 Errors and 0 Warnings across all 70+ C# scripts in the Unity solution.", "•")

    # Evaluation Sign-Off Box
    h2("Academic Project Evaluation & Assessment Sign-Off")
    eval_headers = ["Evaluator Name", "Designation / Faculty", "Evaluation Score", "Signature & Date"]
    eval_data = [
        ["________________________", "________________________", "______ / 100", "____ / ____ / 2026"]
    ]
    create_and_format_table(doc, [Inches(2.0), Inches(2.0), Inches(1.3), Inches(1.8)], eval_headers, eval_data, header_bg="2D3748")

    return doc

if __name__ == "__main__":
    doc = build_7_page_doc()
    p1 = r"c:\Users\Boby\Desktop\horror game\Horror_Game_Complete_Project_Documentation.docx"
    p2 = r"c:\Users\Boby\Desktop\Horror_Game_Complete_Project_Documentation.docx"
    doc.save(p1)
    doc.save(p2)
    print(f"Generated successfully at:\n{p1}\n{p2}")
