namespace SmartInventory.Contracts.Requests
{
    public class GetPagingRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
