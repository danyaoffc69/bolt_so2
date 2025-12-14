using Axlebolt.Bolt.Clans;
using Axlebolt.Standoff.Common;
using System;

namespace Axlebolt.Bolt.Player
{
	public static class BoltPlayerExtension
	{
		public static bool IsLocal(this BoltPlayer player)
		{
			return BoltService<BoltPlayerService>.Instance.Player.Id == player.Id;
		}

       /* public static string GetNameWithClanTag(BoltPlayer player)
        {
            string clanTag;
            var clanService = Bolt.BoltService<BoltClanService>.Instance;

            if (clanService.GetClan().IsInMyClan(player.Id))
            {
                clanTag = clanService.GetClan().Tag;
            }
            else
            {
                clanTag = BoltClanExtension.GetClanTag(player);
            }
            if (!string.IsNullOrEmpty(clanTag))
            {
                return $"<color=#ADADAD>[{clanTag}]</color> {TextUtils.CleanText(player.Name)}";
            }
            return player.Name;
        }*/
    }
}
