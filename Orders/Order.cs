namespace Sim
{
    public enum OrderStatus { Pending, Active, Delivered, Cancelled }

    public class Order
    {
        public Character Customer;
        public House House;
        public DeliveryBot Bot;
        public float Age;
        public OrderStatus Status = OrderStatus.Pending;
    }
}
