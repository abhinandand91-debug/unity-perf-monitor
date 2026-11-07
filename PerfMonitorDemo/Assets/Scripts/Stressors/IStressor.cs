namespace PerfMonitorDemo.Stressors
{
    // Something that puts extra load on the game so the overlay has numbers to show
    public interface IStressor
    {
        string Name { get; }

        // Called every frame, most stressors do nothing here
        void Tick();

        void Reset();
    }
}
