namespace Sim   //DONE
{
    public enum BotState { Idle, ToHouse, PauseAtHouse, ToCharacter, PauseAtCharacter }

    public class DeliveryBot : Agent
    {
        public BotState State = BotState.Idle;
        public Order CurrentOrder;
        float pauseTimer;
        GridPos lastCharacterCell; 

        public DeliveryBot(SimConfig cfg, NavGrid grid, PathRequestManager paths, Stats stats, Vec2 pos)
            : base(cfg, grid, paths, stats, pos, cfg.botRadius, true) { }

        public bool IsIdle => State == BotState.Idle;

        public void AssignOrder(Order o)
        {
            CurrentOrder = o;
            o.Bot = this;
            o.Status = OrderStatus.Active; 
            State = BotState.ToHouse;

            // Request path to a cell adjacent to the house
            RequestPath(c => o.House.IsAdjacentCell(grid, c), grid.WorldToCell(o.House.Position), PathPriority.Normal, null);
        }

        
        public void CancelOrder()
        {
            
            CurrentOrder = null;
            State = BotState.Idle;
            PathPending = false;
        }

         protected override void OnArrived()
        {
            // if at house, start pause timer
            if (State == BotState.ToHouse)
            {
                State = BotState.PauseAtHouse;
                pauseTimer = MyRandom.Range(cfg.pauseMin, cfg.pauseMax);
            }
            // if at character, start pause timer
            else if (State == BotState.ToCharacter)
            {
                State = BotState.PauseAtCharacter;
                pauseTimer = MyRandom.Range(cfg.pauseMin, cfg.pauseMax);
            }
        }

        void GoToCharacter()
        {
            Character ch = CurrentOrder.Customer;
            lastCharacterCell = grid.WorldToCell(ch.Position);

            // Request path to a cell adjacent to the character
            RequestPath(c =>
            {
                GridPos cc = grid.WorldToCell(ch.Position);
                return Math.Abs(c.x - cc.x) <= 1 && Math.Abs(c.y - cc.y) <= 1 && !(c.x == cc.x && c.y == cc.y);
            }, lastCharacterCell, PathPriority.Normal, null);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            if (CurrentOrder == null) {
                return;
            } 
            
            // if at house and path is null, start pause timer
            if (State == BotState.ToHouse && CurrentPath == null)
            {
                State = BotState.PauseAtHouse;
                pauseTimer = UnityEngine.Random.Range(0.3f, 0.5f);

            }

            // if at character and path is null, start pause timer
            if (State == BotState.ToCharacter && CurrentPath == null)
            {
                State = BotState.PauseAtCharacter;
                pauseTimer = UnityEngine.Random.Range(0.3f, 0.5f);
            }

            // if pausing, decrement timer
            if (State == BotState.PauseAtHouse || State == BotState.PauseAtCharacter)
            {
                pauseTimer -= dt;
                if (pauseTimer <= 0)
                {
                    if (State == BotState.PauseAtHouse)
                    {
                        // Request path to a cell adjacent to the character
                        RequestPath(grid.IsAdjacentToCharacter, grid.WorldToCell(CurrentOrder.Character.Position), PathPriority.Normal, path =>
                        {
                            CurrentPath = path;
                            PathPending = false;
                        });
                        State = BotState.ToCharacter;

                    }
                    else if (State == BotState.PauseAtCharacter)
                    {
                        // Notify OrderManager that the order has been delivered
                        CurrentOrder.Status = OrderStatus.Delivered;
                        CurrentOrder = null;
                        State = BotState.Idle;
                    }

                }
            }

        }
    }
}
