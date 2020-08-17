using System.Web.Http;
using AirVinyl.Model;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Select().Expand().Filter().OrderBy().MaxTop(null).Count();

            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel(),
                new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder
                {Namespace = "AirVinyl.Model", ContainerName = "AirVinylContainer"};

            builder.EntitySet<Person>("People");
            builder.EntitySet<RecordStore>("RecordStores");

            var isHighRatedFunction = builder.EntityType<RecordStore>().Function("IsHighRated");
            isHighRatedFunction.Parameter<int>("minimumRating");
            isHighRatedFunction.Returns<bool>();
            isHighRatedFunction.Namespace = "AirVinyl.Functions";

            var areRatedByFunction = builder.EntityType<RecordStore>().Collection.Function("AreRatedBy");
            areRatedByFunction.CollectionParameter<int>("personIds");
            areRatedByFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            areRatedByFunction.Namespace = "AirVinyl.Functions";

            var getHighRatedRecordStores = builder.Function("GetHighRatedRecordStores");
            getHighRatedRecordStores.Parameter<int>("minimumRating");
            getHighRatedRecordStores.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            getHighRatedRecordStores.Namespace = "AirVinyl.Functions";

            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";

            var removeRatingsAction = builder.EntityType<RecordStore>().Collection.Action("RemoveRatings");
            removeRatingsAction.Returns<bool>();
            removeRatingsAction.Parameter<int>("personId");
            removeRatingsAction.Namespace = "AirVinyl.Actions";

            var removeRecordStoreRatingsAction = builder.Action("RemoveRecordStoreRatings");
            removeRecordStoreRatingsAction.Parameter<int>("personId");
            removeRecordStoreRatingsAction.Namespace = "AirVinyl.Actions";

            builder.Singleton<Person>("Tim");

            return builder.GetEdmModel();
        }
    }
}