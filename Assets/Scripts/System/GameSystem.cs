class GameSystem : SingletonBaseWithMono<GameSystem>
{
    void Start()
    {
        SafeZoneSystem.Instance.SetActiveSafeZone(0);
    }
}