using System.Collections.Generic;

namespace Sim   //  DINE
{
    public class OrderManager
    {
        readonly SimConfig cfg;
        readonly List<House> houses;
        readonly List<Character> characters;
        readonly List<DeliveryBot> bots;
        readonly PathRequestManager paths;
        readonly Stats stats;
        readonly List<Order> active = new List<Order>();

        public OrderManager(SimConfig cfg, List<House> houses, List<Character> characters,
                            List<DeliveryBot> bots, PathRequestManager paths, Stats stats)
        {
            this.cfg = cfg; this.houses = houses; this.characters = characters;
            this.bots = bots; this.paths = paths; this.stats = stats;
        }

        //Character calls this when ready to orde
        public void PlaceOrder(Character c)
        {

            //!! add 0 check
            if (houses.Count == 0) {
                return;
            }
            // Find a random house for the order
            House house = houses[UnityEngine.Random.Range(0, houses.Count)];
            Order order = new Order
            {
                Customer = c,
                House = house
            };
            active.Add(order);

            stats.ordersPlaced++;

            DeliveryBot closestBot = null;
            float closestDistance = float.MaxValue;

            // (FIXED) assigning bots in Tick, not here 

        }

        public void Tick(float dt)
        {
            
            if (active.Count > 0)
            {
                // First, age the orders and cancel any that have timed out
                for (int i = active.Count - 1; i >= 0; i--)
                {
                    Order order = active[i];


                    if (order.Status == OrderStatus.Delivered)
                    {
                        OnDelivered(order);
                        continue;
                    }


                    order.Age += dt;

                    if (order.Age > cfg.orderTimeout)       // order time ezxceeded
                    {
                        // Cancel the order
                        order.Status = OrderStatus.Cancelled;
                        order.Customer.OnOrderCancelled();
                        stats.ordersCancelled++;
                        active.RemoveAt(i);
                    }
                }

                // Now we can assign idle bots to pending orders
                foreach (Order order in active)
                {
                    if (order.Status != OrderStatus.Pending){
                        continue; // Skip orders that are not pending
                    }

                    DeliveryBot closestBot = null;
                    float closestDistance = float.MaxValue;

                    foreach (var bot in bots)
                    {
                        if (bot.State == BotState.Idle)
                        {
                            float distance = (order.House.Position - bot.Position).Magnitude;
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestBot = bot;
                            }
                        }
                    }

                    if (closestBot != null)
                    {
                        // Assign the order to the closest idle bot
                        closestBot.AssignOrder(order);
                    }

                    // If no idle bot is available, the order remains pending
                    if (closestBot == null)
                    {
                        order.Status = OrderStatus.Pending;
                    }
                }
                
            }
        }

        public void OnDelivered(Order o) { 
           

            // Update stats
            stats.ordersDelivered++;
            active.Remove(o);
            o.Customer.OnOrderDelivered();

        }
    }
}
