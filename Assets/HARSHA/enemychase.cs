using UnityEngine;

/// <summary>
/// Enemy Chase AI script for Agatha / Walking monster.
/// Inherits full horror mechanics from PatrolAndChaseAI:
/// - Vision Cone / FOV line-of-sight raycasting (no wall-hacking)
/// - Investigation of last-known player position
/// - Flashlight sensitivity (detects player farther when flashlight is on)
/// - Face-snapping jumpscare camera shake and respawn
/// - Patrol waypoints with smooth NavMesh navigation
/// </summary>
public class enemychase : PatrolAndChaseAI
{
}
