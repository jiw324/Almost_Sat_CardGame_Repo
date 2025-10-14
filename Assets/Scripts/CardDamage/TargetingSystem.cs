using System.Linq;
using UnityEngine;

public static class TargetingSystem
{
    public static Actor[] GetAlliesOf(Team ownerTeam)
        => GameObject.FindObjectsOfType<Actor>()
                     .Where(a => a.team == ownerTeam)
                     .ToArray();

    public static Actor[] GetEnemiesOf(Team ownerTeam)
        => GameObject.FindObjectsOfType<Actor>()
                     .Where(a => a.team != ownerTeam && a.team != Team.Neutral)
                     .ToArray();

    public static Actor GetFirstAlly(Team ownerTeam)
        => GetAlliesOf(ownerTeam).FirstOrDefault();

    public static Actor GetFirstEnemy(Team ownerTeam)
        => GetEnemiesOf(ownerTeam).FirstOrDefault();
}
