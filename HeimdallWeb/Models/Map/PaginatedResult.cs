namespace HeimdallWeb.Models.Map
{
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Itens da página atual
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// A quantidade total de itens
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// A página atual (1-based)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// O tamanho da página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// A quantidade total de páginas
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Verifica se existe uma página anterior com expressões lambda
        /// </summary>
        public bool HasPrevious => Page > 1;

        /// <summary>
        /// Verifica se existe uma próxima página com expressões lambda
        /// </summary>
        public bool HasNext => Page < TotalPages;
    }
}
