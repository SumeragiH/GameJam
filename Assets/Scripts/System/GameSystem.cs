class GameSystem : SingletonBaseWithMono<GameSystem>
{
    void Start()
    {
        SafeZoneSystem.Instance.InitActiveSafeZone();
    }
}