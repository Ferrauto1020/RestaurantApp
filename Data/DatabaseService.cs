using System.Data.Common;
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

        public async Task<Order[]> GetOrdersAsync() => await _connection.Table<Order>().ToArrayAsync();

        public async Task<OrderItem[]> GetOrderItemsAsync(long? orderId) => await _connection.Table<OrderItem>().Where(oi => oi.OrderId == orderId).ToArrayAsync();

        public async Task<MenuCategory[]> GetCategoriesOfMenuItem(int menuItemId)
        {
            var query = @"
    SELECT cat.* 
    FROM MenuCategory cat
    INNER JOIN
    MenuItemCategoryMapping map
    ON cat.Id = map.MenuCategoryId
    WHERE map.MenuItemId = ?
    ";
            var categories = await _connection.QueryAsync<MenuCategory>(query, menuItemId);
            return [.. categories];
        }


        public async Task<string?> SaveMenuItemAsync(MenuItemModel model)
        {
            if (model.Id == 0)
            {
                //creating a new item
                MenuItem menuItem = new()
                {
                    Name = model.Name,
                    Description = model.Description,
                    Icon = model.Icon,
                    Price = model.Price
                };
                if (await _connection.InsertAsync(menuItem) > 0)
                {
                    var categoryMapping = model
                    .SelectedCategories
                    .Select(c => new MenuItemCategoryMapping
                    {
                        Id = c.Id,
                        MenuCategoryId = c.Id,
                        MenuItemId = menuItem.Id
                    });
                    if (await _connection.InsertAllAsync(categoryMapping) > 0)
                    {
                        model.Id = menuItem.Id;
                        return null;
                    }
                    else
                    {
                        //something went wrong
                        await _connection.DeleteAsync(menuItem);
                    }
                }
                return "error in saving menu item";
            }
            else
            {
                //updtaing an item
                string? errorMessage = null;

                await _connection.RunInTransactionAsync(db =>
                {
                    var menuItem = db.Find<MenuItem>(model.Id);
                    menuItem.Name = model.Name;
                    menuItem.Description = model.Description;
                    menuItem.Icon = model.Icon;
                    menuItem.Price = model.Price;
                    if (db.Update(menuItem) == 0)
                    {
                        //operation failed 
                        errorMessage = "Error in life choice";
                        throw new Exception();
                    }
                    var deleteQuery = @"
                    DELETE FROM MenuItemCategoryMapping
                    WHERE MenuItemId = ?
                    ";
                    db.Execute(deleteQuery, menuItem.Id);
                    var categoryMapping = model.SelectedCategories
                    .Select(c => new MenuItemCategoryMapping
                    {
                        Id = c.Id,
                        MenuCategoryId = c.Id,
                        MenuItemId = menuItem.Id
                    });
                    if (db.InsertAll(categoryMapping) == 0)
                    {
                        errorMessage = "Error in life choice ";
                        //operation failed we'll get them next time
                        throw new Exception();
                    }
                });
                return errorMessage;
            }
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