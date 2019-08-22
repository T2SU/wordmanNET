namespace wordman.Utils
{
    public static class PageUtils
    {
        public const int LimitPerPage = 50;
        public const int MaxShowPageCount = 9;

        public static int GetMaxPage(int totalCount)
        {
            return (totalCount + LimitPerPage - 1) / LimitPerPage;
        }
    }
}
