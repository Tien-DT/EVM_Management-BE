using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.API.Setup
{
    public static class ODataConfiguration
    {
        public static ODataConventionModelBuilder GetODataModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Contract>("Contracts");
            modelBuilder.EntitySet<HandoverRecord>("HandoverRecords");
            modelBuilder.EntitySet<Order>("Orders");
            modelBuilder.EntitySet<Vehicle>("Vehicles");
            modelBuilder.EntitySet<VehicleModel>("VehicleModels");
            modelBuilder.EntitySet<VehicleVariant>("VehicleVariants");
            modelBuilder.EntitySet<Dealer>("Dealers");
            modelBuilder.EntitySet<Customer>("Customers");
            modelBuilder.EntitySet<Transport>("Transports");
            modelBuilder.EntitySet<Warehouse>("Warehouses");

            return modelBuilder;
        }
    }
}