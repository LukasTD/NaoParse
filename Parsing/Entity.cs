namespace NaoParse.Parsing
{
	public static class EntityId
	{
		public const long Login = 0x1000000000000010;
		public const long Channel = 0x1000000000000001;
		public const long Broadcast = 0x3000000000000000;
		public const long Cards = 0x0001000000000001;
		public const long Characters = 0x0010000000000001;
		public const long Pets = 0x0010010000000001;
		public const long Partners = 0x0010030000000001;
		public const long Npcs = 0x0010f00000000001;
		public const long Guilds = 0x0300000000500000;
		public const long Items = 0x0050000000000001;
		public const long QuestItems = 0x005000f000000001;
		public const long TmpItems = 0x0050f00000000001;
		public const long ServerProps = 0x00a1000000000000;
		public const long AreaEvents = 0x00b0000000000000;
		public const long Parties = 0x0040000000000001;
		public const long Quests = 0x006000f000000001;
		public const long QuestsTmp = 0x0060f00000000001;
		public const long QuestItemOffset = 0x0010000000000000;
		public const long Instances = 0x0100000000000001;
		public const long Nao = 0x0010ffffffffffff;
		public const long Tin = 0x0010fffffffffffe;

		public static bool IsCharacter(long entityId)
		{
			return entityId > Characters && entityId < Pets;
		}
	}
}
