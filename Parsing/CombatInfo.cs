using System;
using System.Collections.Generic;

namespace NaoParse.Parsing
{
	[Flags]
	public enum CombatActionType : byte
	{
		None = 0,
		TakeHit = 1,
		Hit = 2,
		Unk = 3, // no idea what this is
		SimultaneousHit = 6,
		HardHit = 50,
		Defended = 51,
		SpecialHit = 66,
		CounteredHit = 83,
		RangeHit = 114,
		FighterCounter = 115, // bad naming probably
	}

	[Flags]
	public enum TargetOptions : uint
	{
		None = 0,
		Critical = 1,
		CleanHit = 4,
		FirstHit = 8,
		Finished = 16,
		Result = 32,
		KnockDownFinish = 256,
		Smash = 512,
		KnockBack = 1024,
		KnockDown = 2048,
		FinishingHit = 4096,
		FinishingKnockDown = 4368,
		ManaShield = 1048576
	}

	[Flags]
	public enum AttackerOptions : uint
	{
		None = 0,
		KnockBackHit1 = 2,
		KnockBackHit2 = 4,
		UseEffect = 8,
		Result = 32,
		DualWield = 64,
		FirstHit = 1024
	}

	public class CombatActionPayload
	{
		public long CreatureEntityId { get; set; }
		public CombatActionType Type { get; set; }
		public bool IsAttackerAction { get; set; }
		public SkillId SkillId { get; set; }
		public List<TargetOptions> TargetOptions { get; set; }
		public float Damage { get; set; }
		public float WoundDamage { get; set; }
		public int ManaDamage { get; set; }
		public long Attacker { get; set; }
	}
}
