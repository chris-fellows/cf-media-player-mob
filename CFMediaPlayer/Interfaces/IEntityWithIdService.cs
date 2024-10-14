namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Service for entity with Id property
    /// </summary>
    /// <typeparam name="TEntityType"></typeparam>
    /// <typeparam name="TIDType"></typeparam>
    public interface IEntityWithIdService<TEntityType, TIDType>
    {
        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns></returns>
        List<TEntityType> GetAll();

        /// <summary>
        /// Gets entity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntityType? GetById(TIDType id);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntityType entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="id"></param>
        void Delete(TIDType id);
    }
}
