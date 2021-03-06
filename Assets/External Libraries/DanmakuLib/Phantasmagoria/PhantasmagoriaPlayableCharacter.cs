﻿using UnityEngine;
using System.Collections;
using UnityUtilLib;

public class PhantasmagoriaPlayableCharacter : AbstractAgentCharacter {

	/// <summary>
	/// The attack patterns.
	/// </summary>
	[SerializeField]
	private AbstractAttackPattern[] attackPatterns;

	/// <summary>
	/// The shot offset.
	/// </summary>
	[SerializeField]
	private Vector2 shotOffset;
	
	/// <summary>
	/// The shot velocity.
	/// </summary>
	[SerializeField]
	private float shotVelocity;
	
	/// <summary>
	/// The charge rate.
	/// </summary>
	[SerializeField]
	private float chargeRate = 1.0f;

	/// <summary>
	/// The charge capacity regen.
	/// </summary>
	[SerializeField]
	private float chargeCapacityRegen;
	
	/// <summary>
	/// The current charge capacity.
	/// </summary>
	[SerializeField]
	private float currentChargeCapacity;
	
	/// <summary>
	/// The type of the shot.
	/// </summary>
	[SerializeField]
	private ProjectilePrefab shotType;

	private bool charging;
	/// <summary>
	/// Gets a value indicating whether this instance is charging.
	/// </summary>
	/// <value><c>true</c> if this instance is charging; otherwise, <c>false</c>.</value>
	public bool IsCharging {
		get { 
			return charging; 
		}
		set {
			if(charging && !value) {
				SpecialAttack(Mathf.FloorToInt(CurrentChargeLevel));
			}
			charging = value;
		}
	}

	public override bool IsFiring {
		get {
			return base.IsFiring && !IsCharging;
		}
	}
	
	private float chargeLevel = 0f;
	/// <summary>
	/// Gets the current charge level.
	/// </summary>
	/// <value>The current charge level.</value>
	public float CurrentChargeLevel {
		get { 
			return chargeLevel; 
		}
	}
	
	/// <summary>
	/// Gets the max charge level.
	/// </summary>
	/// <value>The max charge level.</value>
	public int MaxChargeLevel {
		get { 
			return attackPatterns.Length + 1; 
		}
	}

	
	/// <summary>
	/// Gets the current charge capacity.
	/// </summary>
	/// <value>The current charge capacity.</value>
	public float CurrentChargeCapacity {
		get {
			return currentChargeCapacity;
		}
	}

	/// <summary>
	/// The death cancel radius.
	/// </summary>
	[SerializeField]
	private float deathCancelRadius;

	public override void Hit(Projectile proj) {
		base.Hit (proj);
		float radius = deathCancelRadius * Util.MaxComponent2(Util.To2D(Transform.lossyScale));
		Projectile[] toCanccel = Field.GetAllBullets (Transform.position, radius);
		for(int i = 0; i < toCanccel.Length; i++) {
			toCanccel[i].Deactivate();
		}
	}
	
	/// <summary>
	/// Specials the attack.
	/// </summary>
	/// <param name="level">Level.</param>
	public virtual void SpecialAttack(int level) {
		int index = level - 1;
		if (index >= 0 && index < attackPatterns.Length) {
			if(attackPatterns[index] != null) {
				attackPatterns[index].Fire();
			} else {
				Debug.Log("Null AttackPattern triggered. Make Sure all AttackPatterns are fully implemented");
			}
		}
		chargeLevel -= level;
		currentChargeCapacity -= level;
	}

	public override void Initialize (AbstractPlayerAgent agent) {
		base.Initialize (agent);
		for(int i = 0; i < attackPatterns.Length; i++)
			if(attackPatterns[i] != null)
				attackPatterns[i].TargetField = Field.TargetField;
	}

	/// <summary>
	/// Fixeds the update.
	/// </summary>
	public override void FixedUpdate() {
		base.FixedUpdate ();
		float dt = Time.fixedDeltaTime;
		currentChargeCapacity += chargeCapacityRegen * dt;
		if(currentChargeCapacity > MaxChargeLevel) {
			currentChargeCapacity = MaxChargeLevel;
		}
		if(charging) {
			chargeLevel += chargeRate * dt;
			if(chargeLevel > currentChargeCapacity)
				chargeLevel = currentChargeCapacity;
		} else {
			FireCheck(dt);
		}
	}

	public override void Fire () {
		Vector2 offset1, offset2, location;
		offset1 = offset2 = shotOffset;
		offset2.x *= -1;
		location = Util.To2D(Transform.position);
		Projectile shot1 = Field.SpawnProjectile(shotType, location + offset1, 0f, FieldCoordinateSystem.AbsoluteWorld);
		Projectile shot2 = Field.SpawnProjectile(shotType, location + offset2, 0f, FieldCoordinateSystem.AbsoluteWorld);
		shot1.Velocity = shot2.Velocity = shotVelocity;
	}
}
