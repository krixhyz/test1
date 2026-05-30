using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // ── 1. Roles ────────────────────────────────────────────────────────────
        foreach (var role in new[] { "Admin", "Staff", "Customer" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        // ── 2. Staff users ──────────────────────────────────────────────────────
        var staffUser1 = await EnsureUser(userManager, "staff1@vparts.com", "Staff@123", "Alice Johnson", "Staff", "Sales Associate");
        var staffUser2 = await EnsureUser(userManager, "staff2@vparts.com", "Staff@123", "Bob Smith",    "Staff", "Parts Manager");

        // ── 3. Customer users + Customer records ────────────────────────────────
        // Only create if the customer email doesn't already exist
        ApplicationUser? custUser1 = await userManager.FindByEmailAsync("customer1@vparts.com");
        ApplicationUser? custUser2 = await userManager.FindByEmailAsync("customer2@vparts.com");
        ApplicationUser? custUser3 = await userManager.FindByEmailAsync("customer3@vparts.com");
        ApplicationUser? custUser4 = await userManager.FindByEmailAsync("customer4@vparts.com");
        ApplicationUser? custUser5 = await userManager.FindByEmailAsync("customer5@vparts.com");

        if (custUser1 == null) { custUser1 = new ApplicationUser { UserName = "customer1@vparts.com", Email = "customer1@vparts.com", FullName = "Ravi Kumar",    PhoneNumber = "9841000001" }; await userManager.CreateAsync(custUser1, "Cust@123"); await userManager.AddToRoleAsync(custUser1, "Customer"); }
        if (custUser2 == null) { custUser2 = new ApplicationUser { UserName = "customer2@vparts.com", Email = "customer2@vparts.com", FullName = "Sita Sharma",   PhoneNumber = "9841000002" }; await userManager.CreateAsync(custUser2, "Cust@123"); await userManager.AddToRoleAsync(custUser2, "Customer"); }
        if (custUser3 == null) { custUser3 = new ApplicationUser { UserName = "customer3@vparts.com", Email = "customer3@vparts.com", FullName = "Hari Thapa",    PhoneNumber = "9841000003" }; await userManager.CreateAsync(custUser3, "Cust@123"); await userManager.AddToRoleAsync(custUser3, "Customer"); }
        if (custUser4 == null) { custUser4 = new ApplicationUser { UserName = "customer4@vparts.com", Email = "customer4@vparts.com", FullName = "Gita Rai",      PhoneNumber = "9841000004" }; await userManager.CreateAsync(custUser4, "Cust@123"); await userManager.AddToRoleAsync(custUser4, "Customer"); }
        if (custUser5 == null) { custUser5 = new ApplicationUser { UserName = "customer5@vparts.com", Email = "customer5@vparts.com", FullName = "Bikash Gurung", PhoneNumber = "9841000005" }; await userManager.CreateAsync(custUser5, "Cust@123"); await userManager.AddToRoleAsync(custUser5, "Customer"); }

        // Customer records
        var cust1 = await EnsureCustomer(db, custUser1.Id, "Kathmandu, Baneshwor");
        var cust2 = await EnsureCustomer(db, custUser2.Id, "Lalitpur, Patan");
        var cust3 = await EnsureCustomer(db, custUser3.Id, "Bhaktapur, Suryabinayak");
        var cust4 = await EnsureCustomer(db, custUser4.Id, "Kathmandu, Thamel");
        var cust5 = await EnsureCustomer(db, custUser5.Id, "Pokhara, Lakeside");

        // ── 4. Customer Vehicles ────────────────────────────────────────────────
        var veh1 = await EnsureVehicle(db, cust1.Id, "BA 1 CHA 1234", "Honda",   "CB Shine",    "Motorcycle");
        var veh2 = await EnsureVehicle(db, cust1.Id, "BA 2 CHA 5678", "Toyota",  "Corolla",     "Car");
        var veh3 = await EnsureVehicle(db, cust2.Id, "BA 3 PA 9012",  "Yamaha",  "FZ-S",        "Motorcycle");
        var veh4 = await EnsureVehicle(db, cust3.Id, "BA 4 CHA 3456", "Hyundai", "i20",         "Car");
        var veh5 = await EnsureVehicle(db, cust4.Id, "BA 5 JA 7890",  "Bajaj",   "Pulsar 150",  "Motorcycle");
        var veh6 = await EnsureVehicle(db, cust5.Id, "GA 1 CHA 1111", "Suzuki",  "Swift",       "Car");

        // ── 5. Vendors ──────────────────────────────────────────────────────────
        var vend1 = await EnsureVendor(db, "AutoParts Nepal Pvt. Ltd.",  "Ramesh Shrestha", "9801111111", "autoparts@nepal.com",   "Kathmandu, New Baneshwor");
        var vend2 = await EnsureVendor(db, "Himalayan Motors Supply",    "Sunita Karki",    "9802222222", "himalayan@motors.com",  "Lalitpur, Kumaripati");
        var vend3 = await EnsureVendor(db, "Everest Auto Components",    "Dipak Tamang",    "9803333333", "everest@autocomp.com",  "Bhaktapur, Katunje");

        // ── 6. Parts ────────────────────────────────────────────────────────────
        var part1  = await EnsurePart(db, "Engine Oil Filter",      "ENG-OIL-001", "Engine",    "High-quality oil filter for 4-stroke engines",          850m,  45, 10, vend1.Id);
        var part2  = await EnsurePart(db, "Brake Pad Set (Front)",  "BRK-PAD-001", "Brakes",    "Front disc brake pads, fits most Japanese cars",        1200m, 30, 8,  vend1.Id);
        var part3  = await EnsurePart(db, "Air Filter",             "AIR-FLT-001", "Engine",    "Dry-type air filter for motorcycles and small cars",     650m,  60, 15, vend2.Id);
        var part4  = await EnsurePart(db, "Spark Plug (NGK)",       "SPK-PLG-001", "Ignition",  "NGK standard spark plug, universal fit",                 350m,  80, 20, vend2.Id);
        var part5  = await EnsurePart(db, "Clutch Cable",           "CLT-CBL-001", "Drivetrain","Replacement clutch cable for motorcycles",               450m,  25, 5,  vend3.Id);
        var part6  = await EnsurePart(db, "Radiator Coolant 1L",    "RAD-CLT-001", "Cooling",   "Concentrated coolant, mix 50/50 with water",             550m,  40, 10, vend1.Id);
        var part7  = await EnsurePart(db, "Timing Belt",            "TIM-BLT-001", "Engine",    "OEM-spec timing belt for 1.3–1.6L engines",             2200m, 15, 4,  vend3.Id);
        var part8  = await EnsurePart(db, "Headlight Bulb H4",      "LGT-BLB-001", "Electrical","H4 halogen bulb 60/55W, pair",                           480m,  50, 12, vend2.Id);
        var part9  = await EnsurePart(db, "Wheel Bearing (Front)",  "WHL-BRG-001", "Suspension","Front wheel bearing kit, fits Toyota/Honda",            1800m, 20, 5,  vend1.Id);
        var part10 = await EnsurePart(db, "Battery 12V 7Ah",        "BAT-12V-001", "Electrical","Maintenance-free sealed lead-acid battery",             3500m, 12, 3,  vend3.Id);
        var part11 = await EnsurePart(db, "Wiper Blade 18\"",       "WPR-BLD-001", "Body",      "18-inch frameless wiper blade",                          380m,  35, 8,  vend2.Id);
        var part12 = await EnsurePart(db, "Fuel Filter",            "FUL-FLT-001", "Fuel",      "Inline fuel filter for petrol engines",                  720m,  28, 7,  vend1.Id);
        // Low-stock part to trigger notifications
        var part13 = await EnsurePart(db, "Throttle Body Gasket",   "THR-GSK-001", "Engine",    "OEM throttle body gasket",                               290m,  3,  5,  vend3.Id);

        // ── 7. Purchase Invoices ────────────────────────────────────────────────
        if (!await db.PurchaseInvoices.AnyAsync())
        {
            var pi1 = new PurchaseInvoice {
                InvoiceNumber = "PUR-20260101001", VendorId = vend1.Id,
                Date = DateTime.UtcNow.AddDays(-60), Notes = "Initial stock purchase",
                Items = new List<PurchaseInvoiceItem> {
                    new() { PartId = part1.Id, Quantity = 20, UnitCost = 700m },
                    new() { PartId = part2.Id, Quantity = 15, UnitCost = 950m },
                    new() { PartId = part6.Id, Quantity = 20, UnitCost = 420m },
                    new() { PartId = part9.Id, Quantity = 10, UnitCost = 1400m },
                }
            };
            pi1.TotalAmount = pi1.Items.Sum(i => i.Quantity * i.UnitCost);

            var pi2 = new PurchaseInvoice {
                InvoiceNumber = "PUR-20260201001", VendorId = vend2.Id,
                Date = DateTime.UtcNow.AddDays(-30), Notes = "Restocking order",
                Items = new List<PurchaseInvoiceItem> {
                    new() { PartId = part3.Id, Quantity = 30, UnitCost = 500m },
                    new() { PartId = part4.Id, Quantity = 40, UnitCost = 270m },
                    new() { PartId = part8.Id, Quantity = 25, UnitCost = 360m },
                    new() { PartId = part11.Id, Quantity = 20, UnitCost = 290m },
                }
            };
            pi2.TotalAmount = pi2.Items.Sum(i => i.Quantity * i.UnitCost);

            var pi3 = new PurchaseInvoice {
                InvoiceNumber = "PUR-20260301001", VendorId = vend3.Id,
                Date = DateTime.UtcNow.AddDays(-10), Notes = "Urgent restock",
                Items = new List<PurchaseInvoiceItem> {
                    new() { PartId = part5.Id,  Quantity = 12, UnitCost = 340m },
                    new() { PartId = part7.Id,  Quantity = 8,  UnitCost = 1700m },
                    new() { PartId = part10.Id, Quantity = 6,  UnitCost = 2800m },
                    new() { PartId = part12.Id, Quantity = 15, UnitCost = 560m },
                }
            };
            pi3.TotalAmount = pi3.Items.Sum(i => i.Quantity * i.UnitCost);

            db.PurchaseInvoices.AddRange(pi1, pi2, pi3);
            await db.SaveChangesAsync();
        }

        // ── 8. Sales Invoices ───────────────────────────────────────────────────
        if (!await db.SalesInvoices.AnyAsync())
        {
            var staffId = staffUser1.Id;

            // Invoice 1 — Paid, subtotal < 5000
            var si1 = new SalesInvoice {
                InvoiceNumber = "INV-20260310001", CustomerId = cust1.Id, StaffId = staffId,
                Date = DateTime.UtcNow.AddDays(-40), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part1.Id, Quantity = 2, UnitPrice = part1.UnitPrice },
                    new() { PartId = part4.Id, Quantity = 4, UnitPrice = part4.UnitPrice },
                }
            };
            si1.Subtotal = si1.Items.Sum(i => i.Quantity * i.UnitPrice);
            si1.Discount = si1.Subtotal > 5000 ? si1.Subtotal * 0.10m : 0;
            si1.TotalAmount = si1.Subtotal - si1.Discount;

            // Invoice 2 — Paid, subtotal > 5000 (loyalty discount triggered)
            var si2 = new SalesInvoice {
                InvoiceNumber = "INV-20260315001", CustomerId = cust2.Id, StaffId = staffId,
                Date = DateTime.UtcNow.AddDays(-35), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part7.Id, Quantity = 2, UnitPrice = part7.UnitPrice },
                    new() { PartId = part9.Id, Quantity = 1, UnitPrice = part9.UnitPrice },
                    new() { PartId = part2.Id, Quantity = 1, UnitPrice = part2.UnitPrice },
                }
            };
            si2.Subtotal = si2.Items.Sum(i => i.Quantity * i.UnitPrice);
            si2.Discount = si2.Subtotal > 5000 ? si2.Subtotal * 0.10m : 0;
            si2.TotalAmount = si2.Subtotal - si2.Discount;

            // Invoice 3 — Credit payment
            var si3 = new SalesInvoice {
                InvoiceNumber = "INV-20260320001", CustomerId = cust3.Id, StaffId = staffId,
                Date = DateTime.UtcNow.AddDays(-20), PaymentStatus = "Credit",
                CreditDueDate = DateTime.UtcNow.AddDays(10),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part10.Id, Quantity = 1, UnitPrice = part10.UnitPrice },
                    new() { PartId = part8.Id,  Quantity = 2, UnitPrice = part8.UnitPrice },
                }
            };
            si3.Subtotal = si3.Items.Sum(i => i.Quantity * i.UnitPrice);
            si3.Discount = si3.Subtotal > 5000 ? si3.Subtotal * 0.10m : 0;
            si3.TotalAmount = si3.Subtotal - si3.Discount;

            // Invoice 4 — Partial payment
            var si4 = new SalesInvoice {
                InvoiceNumber = "INV-20260401001", CustomerId = cust4.Id, StaffId = staffId,
                Date = DateTime.UtcNow.AddDays(-10), PaymentStatus = "Partial",
                CreditDueDate = DateTime.UtcNow.AddDays(20),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part3.Id, Quantity = 2, UnitPrice = part3.UnitPrice },
                    new() { PartId = part6.Id, Quantity = 3, UnitPrice = part6.UnitPrice },
                    new() { PartId = part11.Id, Quantity = 2, UnitPrice = part11.UnitPrice },
                }
            };
            si4.Subtotal = si4.Items.Sum(i => i.Quantity * i.UnitPrice);
            si4.Discount = si4.Subtotal > 5000 ? si4.Subtotal * 0.10m : 0;
            si4.TotalAmount = si4.Subtotal - si4.Discount;

            // Invoice 5 — Recent sale today
            var si5 = new SalesInvoice {
                InvoiceNumber = "INV-20260519001", CustomerId = cust5.Id, StaffId = staffId,
                Date = DateTime.UtcNow, PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part5.Id, Quantity = 1, UnitPrice = part5.UnitPrice },
                    new() { PartId = part12.Id, Quantity = 2, UnitPrice = part12.UnitPrice },
                }
            };
            si5.Subtotal = si5.Items.Sum(i => i.Quantity * i.UnitPrice);
            si5.Discount = si5.Subtotal > 5000 ? si5.Subtotal * 0.10m : 0;
            si5.TotalAmount = si5.Subtotal - si5.Discount;

            db.SalesInvoices.AddRange(si1, si2, si3, si4, si5);
            await db.SaveChangesAsync();
        }

        // ── 9. Appointments ─────────────────────────────────────────────────────
        if (!await db.Appointments.AnyAsync())
        {
            db.Appointments.AddRange(
                new Appointment { CustomerId = cust1.Id, VehicleId = veh1.Id, AppointmentDate = DateTime.UtcNow.AddDays(-30), ServiceType = "Oil Change",        Description = "Full synthetic oil change + filter",    Status = "Completed" },
                new Appointment { CustomerId = cust1.Id, VehicleId = veh2.Id, AppointmentDate = DateTime.UtcNow.AddDays(-15), ServiceType = "Brake Inspection",  Description = "Front brake pad check and replacement",  Status = "Completed" },
                new Appointment { CustomerId = cust2.Id, VehicleId = veh3.Id, AppointmentDate = DateTime.UtcNow.AddDays(-7),  ServiceType = "Tune-Up",           Description = "Spark plug, air filter, throttle clean", Status = "Completed" },
                new Appointment { CustomerId = cust3.Id, VehicleId = veh4.Id, AppointmentDate = DateTime.UtcNow.AddDays(2),   ServiceType = "Timing Belt",       Description = "Timing belt replacement at 60k km",      Status = "Pending"   },
                new Appointment { CustomerId = cust4.Id, VehicleId = veh5.Id, AppointmentDate = DateTime.UtcNow.AddDays(5),   ServiceType = "General Service",   Description = "Full service check",                     Status = "Pending"   },
                new Appointment { CustomerId = cust5.Id, VehicleId = veh6.Id, AppointmentDate = DateTime.UtcNow.AddDays(-3),  ServiceType = "Battery Check",     Description = "Battery voltage test and replacement",   Status = "Completed" }
            );
            await db.SaveChangesAsync();
        }

        // ── 10. Reviews ─────────────────────────────────────────────────────────
        if (!await db.Reviews.AnyAsync())
        {
            var appts = await db.Appointments.Where(a => a.Status == "Completed").ToListAsync();
            var reviews = new List<Review>();
            if (appts.Count > 0) reviews.Add(new Review { CustomerId = cust1.Id, AppointmentId = appts[0].Id, Rating = 5, Comment = "Excellent service, very professional staff!", Date = DateTime.UtcNow.AddDays(-28) });
            if (appts.Count > 1) reviews.Add(new Review { CustomerId = cust1.Id, AppointmentId = appts[1].Id, Rating = 4, Comment = "Good work on the brakes, quick turnaround.",   Date = DateTime.UtcNow.AddDays(-13) });
            if (appts.Count > 2) reviews.Add(new Review { CustomerId = cust2.Id, AppointmentId = appts[2].Id, Rating = 5, Comment = "Bike runs perfectly after the tune-up.",        Date = DateTime.UtcNow.AddDays(-5)  });
            if (appts.Count > 3) reviews.Add(new Review { CustomerId = cust5.Id, AppointmentId = appts[3].Id, Rating = 3, Comment = "Battery replaced but took longer than expected.", Date = DateTime.UtcNow.AddDays(-1) });
            if (reviews.Count > 0) { db.Reviews.AddRange(reviews); await db.SaveChangesAsync(); }
        }

        // ── 11. Unavailable Part Requests ───────────────────────────────────────
        if (!await db.UnavailablePartRequests.AnyAsync())
        {
            db.UnavailablePartRequests.AddRange(
                new UnavailablePartRequest { CustomerId = cust1.Id, VehicleId = veh2.Id, PartName = "Catalytic Converter",    Description = "Need OEM cat converter for Corolla 2018", Urgency = "High",   Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-5) },
                new UnavailablePartRequest { CustomerId = cust3.Id, VehicleId = veh4.Id, PartName = "ABS Sensor (Rear)",      Description = "Rear ABS sensor for i20 2020",            Urgency = "Medium", Status = "InProgress", RequestDate = DateTime.UtcNow.AddDays(-3) },
                new UnavailablePartRequest { CustomerId = cust4.Id, VehicleId = veh5.Id, PartName = "Carburetor Jet Kit",     Description = "Performance jet kit for Pulsar 150",      Urgency = "Low",    Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-1) }
            );
            await db.SaveChangesAsync();
        }

        // ── 12. Service History ─────────────────────────────────────────────────
        if (!await db.ServiceHistories.AnyAsync())
        {
            db.ServiceHistories.AddRange(
                new ServiceHistory { CustomerId = cust1.Id, VehicleId = veh1.Id, ServiceType = "Oil Change",       Description = "Synthetic oil + filter replaced",         Technician = "Alice Johnson", Cost = 1200m,  ServiceDate = DateTime.UtcNow.AddDays(-30), Status = "Completed" },
                new ServiceHistory { CustomerId = cust1.Id, VehicleId = veh2.Id, ServiceType = "Brake Service",    Description = "Front brake pads replaced",               Technician = "Bob Smith",     Cost = 2400m,  ServiceDate = DateTime.UtcNow.AddDays(-15), Status = "Completed" },
                new ServiceHistory { CustomerId = cust2.Id, VehicleId = veh3.Id, ServiceType = "Tune-Up",          Description = "Spark plug + air filter + throttle clean", Technician = "Alice Johnson", Cost = 1800m,  ServiceDate = DateTime.UtcNow.AddDays(-7),  Status = "Completed" },
                new ServiceHistory { CustomerId = cust5.Id, VehicleId = veh6.Id, ServiceType = "Battery Replaced", Description = "12V 7Ah battery installed",               Technician = "Bob Smith",     Cost = 3800m,  ServiceDate = DateTime.UtcNow.AddDays(-3),  Status = "Completed" }
            );
            await db.SaveChangesAsync();
        }

        // ── 13. Notifications ───────────────────────────────────────────────────
        if (!await db.Notifications.AnyAsync())
        {
            db.Notifications.AddRange(
                new Notification { Title = "Low Stock Alert",    Message = "Throttle Body Gasket stock is critically low (3 remaining).", Type = "LowStock",  IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new Notification { Title = "New Part Request",   Message = "Customer Ravi Kumar requested Catalytic Converter.",          Type = "Request",   IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Notification { Title = "Credit Due Soon",    Message = "Invoice INV-20260320001 credit due in 10 days.",              Type = "Credit",    IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Notification { Title = "New Review",         Message = "Ravi Kumar left a 5-star review.",                           Type = "Review",    IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-28) }
            );
            await db.SaveChangesAsync();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static async Task<ApplicationUser> EnsureUser(
        UserManager<ApplicationUser> um, string email, string password,
        string fullName, string role, string position = "")
    {
        var user = await um.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, Position = position };
            await um.CreateAsync(user, password);
            await um.AddToRoleAsync(user, role);
        }
        return user;
    }

    private static async Task<Customer> EnsureCustomer(ApplicationDbContext db, Guid userId, string address)
    {
        var existing = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (existing != null) return existing;
        var c = new Customer { UserId = userId, Address = address };
        db.Customers.Add(c);
        await db.SaveChangesAsync();
        return c;
    }

    private static async Task<CustomerVehicle> EnsureVehicle(
        ApplicationDbContext db, Guid customerId, string vehicleNumber,
        string brand, string model, string vehicleType)
    {
        var existing = await db.CustomerVehicles.FirstOrDefaultAsync(v => v.VehicleNumber == vehicleNumber);
        if (existing != null) return existing;
        var v = new CustomerVehicle { CustomerId = customerId, VehicleNumber = vehicleNumber, Brand = brand, Model = model, VehicleType = vehicleType };
        db.CustomerVehicles.Add(v);
        await db.SaveChangesAsync();
        return v;
    }

    private static async Task<Vendor> EnsureVendor(
        ApplicationDbContext db, string name, string contact, string phone, string email, string address)
    {
        var existing = await db.Vendors.FirstOrDefaultAsync(v => v.VendorName == name);
        if (existing != null) return existing;
        var v = new Vendor { VendorName = name, ContactPerson = contact, Phone = phone, Email = email, Address = address };
        db.Vendors.Add(v);
        await db.SaveChangesAsync();
        return v;
    }

    private static async Task<Part> EnsurePart(
        ApplicationDbContext db, string name, string code, string category,
        string description, decimal price, int stock, int reorder, Guid vendorId)
    {
        var existing = await db.Parts.FirstOrDefaultAsync(p => p.PartCode == code);
        if (existing != null) return existing;
        var p = new Part { PartName = name, PartCode = code, Category = category, Description = description, UnitPrice = price, StockQuantity = stock, ReorderLevel = reorder, VendorId = vendorId };
        db.Parts.Add(p);
        await db.SaveChangesAsync();
        return p;
    }
}
