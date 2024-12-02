namespace StreamUP
{
    public partial class StreamUpLib
    {
        public DSIStateInfo StateInfo { get; set; }

        public StreamUpLib()
        {
            StateInfo = new DSIStateInfo();
        }

        // A method to set the state and perform state-specific logic
        public void DSISetState(string widget, DSIStateInfo.DSIState state)
        {
            LogDebug($"Setting state to: {state}, Active Widget: {widget}");

            // Ensure StateInfo is not null
            if (StateInfo == null)
            {
                LogError("StateInfo is null. Initializing StateInfo.");
                StateInfo = new DSIStateInfo();
            }

            // Update the current state and active widget
            StateInfo.CurrentState = state;
            StateInfo.ActiveWidget = widget;

            // Perform actions based on the new state
            switch (state)
            {
                case DSIStateInfo.DSIState.Default:
                    LogDebug("Handling Default state.");
                    HandleDefaultState();
                    break;
                case DSIStateInfo.DSIState.Locked:
                    HandleLockedState();
                    break;
                case DSIStateInfo.DSIState.Static:
                    HandleStaticState();
                    break;
                case DSIStateInfo.DSIState.Rotating:
                    HandleRotatingState();
                    break;
                case DSIStateInfo.DSIState.Transitioning:
                    HandleTransitioningState();
                    break;
                default:
                    HandleDefaultState();
                    break;
            }

            LogDebug($"State changed to: {StateInfo.CurrentState}, Active Widget: {StateInfo.ActiveWidget}");
        }

        private void HandleDefaultState()
        {
            LogDebug("Handling Default state.");
            // Add logic for Default state
        }

        private void HandleLockedState()
        {
            LogDebug("Handling Locked state.");
            // Add logic for Locked state
        }

        private void HandleStaticState()
        {
            LogDebug("Handling Static state.");
            // Add logic for Static state
        }

        private void HandleRotatingState()
        {
            LogDebug("Handling Rotating state.");
            // Add logic for Rotating state
        }

        private void HandleTransitioningState()
        {
            LogDebug("Handling Transitioning state.");
            // Add logic for Transitioning state
        }
    }

    public class DSIStateInfo
    {
        public enum DSIState
        {
            Default,
            Locked,
            Static,
            Rotating,
            Transitioning
        }

        public DSIState CurrentState { get; set; }
        public string ActiveWidget { get; set; }

        public DSIStateInfo()
        {
            CurrentState = DSIState.Default;
            ActiveWidget = string.Empty;
        }
    }
}
