namespace Network.Sim.Scenarios {
    /// <summary>
    /// Represents a simulation scenario.
    /// </summary>
    public abstract class Scenario {
        /// <summary>
        /// The friendly name of the scenario.
        /// </summary>
        public abstract string Name {
            get;
        }

        /// <summary>
        /// A description of the scenario.
        /// </summary>
        public abstract string Description {
            get;
        }

        /// <summary>
        /// Runs the scenario.
        /// </summary>
        public abstract void Run();
    }
}