class GameSystem : SingletonBaseWithMono<GameSystem>
{
    void Start()
    {
        SafeZoneSystem.Instance.setActiveSafeZone(0);
    }
}