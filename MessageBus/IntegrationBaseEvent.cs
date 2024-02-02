namespace MessageBus
{
    public class IntegrationBaseEvent
    {
        public IntegrationBaseEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
        }

        public IntegrationBaseEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public Guid Id { get; set; }
        public DateTime CreationDate { get; private set; }
        public bool IsSuccess { get; set; } = true;
        public string Exception { get; set; }
    }
}
