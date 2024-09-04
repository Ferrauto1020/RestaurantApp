using RestaurantApp.Models;
using SQLite;

namespace RestaurantApp.Data
{
    public class DatabaseService : IAsyncDisposable
    {
        private readonly SQLiteAsyncConnection _connection;
        public DatabaseService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "restaurant");
            _connection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

        }

        public async Task InitializeDatabaseAsync()
        {
            await _connection.CreateTableAsync<MenuCategory>();
            await _connection.CreateTableAsync<MenuItem>();
            await _connection.CreateTableAsync<MenuItemCategoryMapping>();
            await _connection.CreateTableAsync<Order>();
            await _connection.CreateTableAsync<OrderItem>();

            await SeedDataAsync();
        }
        private async Task SeedDataAsync()
        {
            var firstCategory = await _connection.Table<MenuCategory>().FirstOrDefaultAsync();
            if (firstCategory != null)
            {
                return; //db already seeded
            }
            var categories = SeedData.GetMenuCategories();
            var menuItems = SeedData.GetMenuItems();
            var mappings = SeedData.GetMenuItemCategoryMappings();

            await _connection.InsertAllAsync(categories);
            await _connection.InsertAllAsync(menuItems);
            await _connection.InsertAllAsync(mappings);
        }

        public async Task<MenuCategory[]> GetMenuCategoriesAsync()
        {
            return await _connection.Table<MenuCategory>().ToArrayAsync();
        }

        public async Task<MenuItem[]> GetMenuItemsByCategoryAsync(int categoryId)
        {
            var query = @"
            SELECT menu.*
            FROM MenuItem AS menu
            INNER JOIN MenuItemCategoryMapping AS mapping 
            ON menu.Id = mapping.MenuItemId
            WHERE mapping.MenuCategoryId = ?
            ";
            var menuItems = await _connection.QueryAsync<MenuItem>(query, categoryId);
            return [.. menuItems];
        }

        public async Task<string?> PlaceOrderAsync(OrderModel model)
        {
            var order = new Order
            {
                OrderDate = model.OrderDate,
                PaymentMode = model.PaymentMode,
                TotalAmountPaid = model.TotalAmountPaid,
                TotalItemsCount = model.TotalItemsCount

            };
            if (await _connection.InsertAsync(order) > 0)
            {
                //order inserted
                foreach (var item in model.Items)
                {
                    item.OrderId = order.Id;
                }
                if (await _connection.InsertAllAsync(model.Items) == 0)
                {
                    //orderItem insert operation failed
                    await _connection.DeleteAsync(order);
                    return "Error in inserting order items";
                }
            }
            else
            {
                return "error in inserting the order";
            }
            model.Id = order.Id;
            return null;
        }
        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                _connection.CloseAsync();
            }
        }
    }
}