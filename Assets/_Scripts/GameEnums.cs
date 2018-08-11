using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JUMPSTATUS
{
    None,
    NormalJump,
    WallJump,
    WaterJump,
    DoubleJump,
    LadderJump,
    SwordJump
}

public enum PlayerHealthStatus
{
    Default,
    IsHurt,
    IsDying
}

public enum PlayerAnimationStatus
{
    Default,
    IsGroundSliding,
    IsDodging
}

public enum EnemyMovementStatus
{
    None,
    IsMoving,
    IsTakingDamage,
    IsChargingAttack,
    IsAttackingPlayer
}


