using System;
using System.Linq;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class DriverStockService
    {
        /// <summary>
        /// Inserts the driver stock.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="trailerNumber">Trailer number.</param>
        /// <param name="product">Product.</param>
        public static void InsertDriverStock(Repository repository, string trailerNumber, Product product)
        {
            DriverStock trailerStock = CreateTrailerStockItem(repository, trailerNumber, product);
            repository.DriverStock.Add(trailerStock);
            repository.SaveChanges();
        }

        /// <summary>
        /// Creates the trailer stock item.
        /// </summary>
        /// <returns>The trailer stock item.</returns>
        /// <param name="repository">Repository.</param>
        /// <param name="trailerNumber">Trailer number.</param>
        /// <param name="product">Product.</param>
        public static DriverStock CreateTrailerStockItem(Repository repository, string trailerNumber, Product product)
        {
            var profile = repository.Profiles.First();
            var subscriberID = profile.SubscriberID;

            var id = String.Format("{0}{1}{2}", profile.CurrentTrailerNumber,
                                   product.ProductCode, profile.SubscriberID);

            var sequence = product.Sequence.GetValueOrDefault();

            var trailerStock = new DriverStock
            {
                ProductCode = product.ProductCode,
                TrailerNumber = trailerNumber,
                Fulls = product.Fulls.GetValueOrDefault(),
                Empties = product.Empties.GetValueOrDefault(),
                FaultyFulls = product.FaultyFulls.GetValueOrDefault(),
                FaultyEmpties = product.FaultyEmpties.GetValueOrDefault(),
                SubscriberID = subscriberID,
                ShortDescription = product.ShortDescription,
                GallonsKilosPerFill = product.GallonsKilosPerFill,
                GrossWeight = product.GrossWeight,
                Sequence = sequence,
                Id = id
            };
            return trailerStock;
        }

        /// <summary>
        /// Updates the driver stock.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="id">Identifier.</param>
        /// <param name="product">Product.</param>
        public static void UpdateDriverStock(Repository repository, string id, Product product)
        {
            var trailerStock = repository.DriverStock.Find(id);

            if (trailerStock != null)
            {
                trailerStock.Fulls = product.Fulls.GetValueOrDefault();
                trailerStock.Empties = product.Empties.GetValueOrDefault();
                trailerStock.FaultyFulls = product.FaultyFulls.GetValueOrDefault();
                trailerStock.FaultyEmpties = product.FaultyEmpties.GetValueOrDefault();
                repository.SaveChanges();
            }
        }
    }
}
