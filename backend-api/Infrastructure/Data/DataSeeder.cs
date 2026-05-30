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
        var staffUser3 = await EnsureUser(userManager, "staff3@vparts.com", "Staff@123", "Carol Thapa",  "Staff", "Service Advisor");

        // ── 3. Customer users + Customer records ────────────────────────────────
        ApplicationUser? custUser1  = await userManager.FindByEmailAsync("customer1@vparts.com");
        ApplicationUser? custUser2  = await userManager.FindByEmailAsync("customer2@vparts.com");
        ApplicationUser? custUser3  = await userManager.FindByEmailAsync("customer3@vparts.com");
        ApplicationUser? custUser4  = await userManager.FindByEmailAsync("customer4@vparts.com");
        ApplicationUser? custUser5  = await userManager.FindByEmailAsync("customer5@vparts.com");
        ApplicationUser? custUser6  = await userManager.FindByEmailAsync("customer6@vparts.com");
        ApplicationUser? custUser7  = await userManager.FindByEmailAsync("customer7@vparts.com");
        ApplicationUser? custUser10 = await userManager.FindByEmailAsync("customer10@vparts.com");

        if (custUser1  == null) { custUser1  = new ApplicationUser { UserName = "customer1@vparts.com",  Email = "customer1@vparts.com",  FullName = "Ravi Kumar",          PhoneNumber = "9841000001" }; await userManager.CreateAsync(custUser1,  "Cust@123"); await userManager.AddToRoleAsync(custUser1,  "Customer"); }
        if (custUser2  == null) { custUser2  = new ApplicationUser { UserName = "customer2@vparts.com",  Email = "customer2@vparts.com",  FullName = "Sita Sharma",         PhoneNumber = "9841000002" }; await userManager.CreateAsync(custUser2,  "Cust@123"); await userManager.AddToRoleAsync(custUser2,  "Customer"); }
        if (custUser3  == null) { custUser3  = new ApplicationUser { UserName = "customer3@vparts.com",  Email = "customer3@vparts.com",  FullName = "Hari Thapa",          PhoneNumber = "9841000003" }; await userManager.CreateAsync(custUser3,  "Cust@123"); await userManager.AddToRoleAsync(custUser3,  "Customer"); }
        if (custUser4  == null) { custUser4  = new ApplicationUser { UserName = "customer4@vparts.com",  Email = "customer4@vparts.com",  FullName = "Gita Rai",            PhoneNumber = "9841000004" }; await userManager.CreateAsync(custUser4,  "Cust@123"); await userManager.AddToRoleAsync(custUser4,  "Customer"); }
        if (custUser5  == null) { custUser5  = new ApplicationUser { UserName = "customer5@vparts.com",  Email = "customer5@vparts.com",  FullName = "Bikash Gurung",       PhoneNumber = "9841000005" }; await userManager.CreateAsync(custUser5,  "Cust@123"); await userManager.AddToRoleAsync(custUser5,  "Customer"); }
        if (custUser6  == null) { custUser6  = new ApplicationUser { UserName = "customer6@vparts.com",  Email = "customer6@vparts.com",  FullName = "Ram Bahadur Kshetri", PhoneNumber = "9841000006" }; await userManager.CreateAsync(custUser6,  "Cust@123"); await userManager.AddToRoleAsync(custUser6,  "Customer"); }
        if (custUser7  == null) { custUser7  = new ApplicationUser { UserName = "customer7@vparts.com",  Email = "customer7@vparts.com",  FullName = "Sarita Adhikari",     PhoneNumber = "9841000007" }; await userManager.CreateAsync(custUser7,  "Cust@123"); await userManager.AddToRoleAsync(custUser7,  "Customer"); }
        if (custUser10 == null) { custUser10 = new ApplicationUser { UserName = "customer10@vparts.com", Email = "customer10@vparts.com", FullName = "Anita Poudel",        PhoneNumber = "9841000010" }; await userManager.CreateAsync(custUser10, "Cust@123"); await userManager.AddToRoleAsync(custUser10, "Customer"); }

        // Overdue-demo customers — both emails route to krixh.dhakal@gmail.com.
        // RequireUniqueEmail is false so two users can share an email address.
        // Use FindByNameAsync (username) not FindByEmailAsync to avoid finding each other.
        ApplicationUser? custUser8 = await userManager.FindByNameAsync("creditdemo1@vparts.com");
        ApplicationUser? custUser9 = await userManager.FindByNameAsync("creditdemo2@vparts.com");

        if (custUser8 == null) { custUser8 = new ApplicationUser { UserName = "creditdemo1@vparts.com", Email = "krixh.dhakal@gmail.com", FullName = "Bijaya Magar",  PhoneNumber = "9841000008", EmailConfirmed = true }; await userManager.CreateAsync(custUser8, "Cust@123"); await userManager.AddToRoleAsync(custUser8, "Customer"); }
        if (custUser9 == null) { custUser9 = new ApplicationUser { UserName = "creditdemo2@vparts.com", Email = "krixh.dhakal@gmail.com", FullName = "Suresh Tamang", PhoneNumber = "9841000009", EmailConfirmed = true }; await userManager.CreateAsync(custUser9, "Cust@123"); await userManager.AddToRoleAsync(custUser9, "Customer"); }

        // Customer records
        var cust1  = await EnsureCustomer(db, custUser1.Id,  "Kathmandu, Baneshwor");
        var cust2  = await EnsureCustomer(db, custUser2.Id,  "Lalitpur, Patan");
        var cust3  = await EnsureCustomer(db, custUser3.Id,  "Bhaktapur, Suryabinayak");
        var cust4  = await EnsureCustomer(db, custUser4.Id,  "Kathmandu, Thamel");
        var cust5  = await EnsureCustomer(db, custUser5.Id,  "Pokhara, Lakeside");
        var cust6  = await EnsureCustomer(db, custUser6.Id,  "Lalitpur, Imadol");
        var cust7  = await EnsureCustomer(db, custUser7.Id,  "Kathmandu, Koteshwor");
        var cust8  = await EnsureCustomer(db, custUser8.Id,  "Bhaktapur, Thimi");
        var cust9  = await EnsureCustomer(db, custUser9.Id,  "Pokhara, Bagar");
        var cust10 = await EnsureCustomer(db, custUser10.Id, "Chitwan, Bharatpur");

        // ── 4. Customer Vehicles ────────────────────────────────────────────────
        await EnsureVehicle(db, cust1.Id,  "BA 1 CHA 1234", "Honda",    "CB Shine",      "Motorcycle");
        await EnsureVehicle(db, cust1.Id,  "BA 2 CHA 5678", "Toyota",   "Corolla",       "Car");
        await EnsureVehicle(db, cust2.Id,  "BA 3 PA 9012",  "Yamaha",   "FZ-S",          "Motorcycle");
        await EnsureVehicle(db, cust3.Id,  "BA 4 CHA 3456", "Hyundai",  "i20",           "Car");
        await EnsureVehicle(db, cust4.Id,  "BA 5 JA 7890",  "Bajaj",    "Pulsar 150",    "Motorcycle");
        await EnsureVehicle(db, cust5.Id,  "GA 1 CHA 1111", "Suzuki",   "Swift",         "Car");
        await EnsureVehicle(db, cust6.Id,  "BA 7 CHA 2222", "Mahindra", "Thar",          "SUV");
        await EnsureVehicle(db, cust7.Id,  "BA 8 JA 3333",  "KTM",      "Duke 200",      "Motorcycle");
        await EnsureVehicle(db, cust8.Id,  "GA 2 CHA 4444", "Honda",    "Activa 6G",     "Scooter");
        await EnsureVehicle(db, cust9.Id,  "GA 3 PA 5555",  "Hero",     "Splendor Plus", "Motorcycle");
        await EnsureVehicle(db, cust10.Id, "BA 9 CHA 6666", "Tata",     "Nexon",         "SUV");

        // ── 5. Vendors ──────────────────────────────────────────────────────────
        var vend1  = await EnsureVendor(db, "AutoParts Nepal Pvt. Ltd.",  "Ramesh Shrestha",   "9801111111", "autoparts@nepal.com",           "Kathmandu, New Baneshwor");
        var vend2  = await EnsureVendor(db, "Himalayan Motors Supply",    "Sunita Karki",      "9802222222", "himalayan@motors.com",          "Lalitpur, Kumaripati");
        var vend3  = await EnsureVendor(db, "Everest Auto Components",    "Dipak Tamang",      "9803333333", "everest@autocomp.com",          "Bhaktapur, Katunje");
        var vend4  = await EnsureVendor(db, "Nepal Motor Parts House",    "Laxman Bhattarai",  "9804444444", "nepal.motorparts@gmail.com",    "Kathmandu, Kalanki");
        var vend5  = await EnsureVendor(db, "Kathmandu Auto Supplies",    "Pratima Maharjan",  "9805555555", "ktm.autosupplies@yahoo.com",    "Lalitpur, Jawalakhel");
        var vend6  = await EnsureVendor(db, "Pokhara Vehicle Parts",      "Naresh Gurung",     "9806666666", "pokhara.vparts@gmail.com",      "Pokhara, Prithvi Chowk");
        var vend7  = await EnsureVendor(db, "Birgunj Parts Depot",        "Ramesh Yadav",      "9807777777", "birgunj.parts@gmail.com",       "Birgunj, Ghantaghar");
        var vend8  = await EnsureVendor(db, "Butwal Mechanics Supply",    "Indira Thapa Magar","9808888888", "butwal.mech@nepal.com",         "Butwal, Traffic Chowk");
        var vend9  = await EnsureVendor(db, "Janakpur Auto Center",       "Sanjay Shah",       "9809999999", "janakpur.auto@yahoo.com",       "Janakpur, Station Road");
        var vend10 = await EnsureVendor(db, "Dharan Auto Works",          "Kamala Rai",        "9800000001", "dharan.autoworks@gmail.com",    "Dharan, BP Chowk");

        // ── 6. Parts ────────────────────────────────────────────────────────────
        var part1  = await EnsurePart(db, "Engine Oil Filter",            "ENG-OIL-001", "Engine",    "High-quality oil filter for 4-stroke engines",          850m,  45, 10, vend1.Id);
        var part2  = await EnsurePart(db, "Brake Pad Set (Front)",        "BRK-PAD-001", "Brakes",    "Front disc brake pads, fits most Japanese cars",        1200m, 30,  8, vend1.Id);
        var part3  = await EnsurePart(db, "Air Filter",                   "AIR-FLT-001", "Engine",    "Dry-type air filter for motorcycles and small cars",     650m,  60, 15, vend2.Id);
        var part4  = await EnsurePart(db, "Spark Plug (NGK)",             "SPK-PLG-001", "Ignition",  "NGK standard spark plug, universal fit",                 350m,  80, 20, vend2.Id);
        var part5  = await EnsurePart(db, "Clutch Cable",                 "CLT-CBL-001", "Drivetrain","Replacement clutch cable for motorcycles",               450m,  25,  5, vend3.Id);
        var part6  = await EnsurePart(db, "Radiator Coolant 1L",          "RAD-CLT-001", "Cooling",   "Concentrated coolant, mix 50/50 with water",             550m,  40, 10, vend1.Id);
        var part7  = await EnsurePart(db, "Timing Belt",                  "TIM-BLT-001", "Engine",    "OEM-spec timing belt for 1.3-1.6L engines",             2200m, 15,  4, vend3.Id);
        var part8  = await EnsurePart(db, "Headlight Bulb H4",            "LGT-BLB-001", "Electrical","H4 halogen bulb 60/55W, pair",                           480m,  50, 12, vend2.Id);
        var part9  = await EnsurePart(db, "Wheel Bearing (Front)",        "WHL-BRG-001", "Suspension","Front wheel bearing kit, fits Toyota/Honda",            1800m,  20,  5, vend1.Id);
        var part10 = await EnsurePart(db, "Battery 12V 7Ah",              "BAT-12V-001", "Electrical","Maintenance-free sealed lead-acid battery",             3500m,  12,  3, vend3.Id);
        var part11 = await EnsurePart(db, "Wiper Blade 18\"",             "WPR-BLD-001", "Body",      "18-inch frameless wiper blade",                          380m,  35,  8, vend2.Id);
        var part12 = await EnsurePart(db, "Fuel Filter",                  "FUL-FLT-001", "Fuel",      "Inline fuel filter for petrol engines",                  720m,  28,  7, vend1.Id);
        // Low-stock parts (stock < reorder level — trigger F15 alert notifications)
        var part13 = await EnsurePart(db, "Throttle Body Gasket",         "THR-GSK-001", "Engine",    "OEM throttle body gasket",                               290m,   3,  5, vend3.Id);
        var part14 = await EnsurePart(db, "Suspension Bush Kit",          "SUS-BSH-001", "Suspension","Front suspension bush set, polyurethane",               1650m,   7, 10, vend4.Id);
        var part15 = await EnsurePart(db, "Coolant Temperature Sensor",   "CLT-SEN-001", "Cooling",   "OEM coolant temp sensor, fits Honda/Toyota/Hyundai",   2100m,   9, 12, vend5.Id);

        // ── 7. Purchase Invoices ────────────────────────────────────────────────
        // Original 3 invoices — skip if table already has data
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
                    new() { PartId = part3.Id,  Quantity = 30, UnitCost = 500m },
                    new() { PartId = part4.Id,  Quantity = 40, UnitCost = 270m },
                    new() { PartId = part8.Id,  Quantity = 25, UnitCost = 360m },
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

        // Additional purchase invoices — spread across this month and last month
        await EnsurePurchaseInvoice(db, "PUR-20260502001", vend4.Id, DateTime.UtcNow.AddDays(-28),
            "Suspension and cooling parts stock",
            new[] { (part14.Id, 12, 1250m), (part15.Id, 10, 1600m) });

        await EnsurePurchaseInvoice(db, "PUR-20260508001", vend1.Id, DateTime.UtcNow.AddDays(-22),
            "Engine parts bulk order",
            new[] { (part1.Id, 25, 680m), (part3.Id, 20, 490m), (part7.Id, 10, 1700m) });

        await EnsurePurchaseInvoice(db, "PUR-20260512001", vend5.Id, DateTime.UtcNow.AddDays(-18),
            "Electrical and ignition spares",
            new[] { (part4.Id, 50, 260m), (part8.Id, 30, 350m) });

        await EnsurePurchaseInvoice(db, "PUR-20260518001", vend2.Id, DateTime.UtcNow.AddDays(-12),
            "Brake and drivetrain components",
            new[] { (part2.Id, 20, 940m), (part5.Id, 15, 335m), (part9.Id, 12, 1400m) });

        await EnsurePurchaseInvoice(db, "PUR-20260405001", vend6.Id, DateTime.UtcNow.AddDays(-55),
            "April fuel and cooling restock",
            new[] { (part6.Id, 30, 415m), (part12.Id, 20, 550m) });

        await EnsurePurchaseInvoice(db, "PUR-20260415001", vend3.Id, DateTime.UtcNow.AddDays(-45),
            "April heavy components order",
            new[] { (part10.Id, 8, 2750m), (part7.Id, 6, 1680m), (part14.Id, 8, 1230m) });

        await EnsurePurchaseInvoice(db, "PUR-20260425001", vend7.Id, DateTime.UtcNow.AddDays(-35),
            "April miscellaneous stock",
            new[] { (part11.Id, 25, 280m), (part13.Id, 5, 210m), (part15.Id, 7, 1580m) });

        // ── 8. Sales Invoices ───────────────────────────────────────────────────
        // Each invoice is guarded individually so they are inserted even when other
        // invoices (from other branches/students) already exist in the table.

        // si1 — Paid, subtotal < 5,000, no discount (F16 no-discount contrast)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260310001"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260310001", CustomerId = cust1.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-40), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part1.Id, Quantity = 2, UnitPrice = part1.UnitPrice },
                    new() { PartId = part4.Id, Quantity = 4, UnitPrice = part4.UnitPrice },
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice);
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // si2 — Paid, subtotal > 5,000, 10% loyalty discount (F16 demo #1)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260315001"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260315001", CustomerId = cust2.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-35), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part7.Id, Quantity = 2, UnitPrice = part7.UnitPrice }, // Rs 4,400
                    new() { PartId = part9.Id, Quantity = 1, UnitPrice = part9.UnitPrice }, // Rs 1,800
                    new() { PartId = part2.Id, Quantity = 1, UnitPrice = part2.UnitPrice }, // Rs 1,200
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 7,400
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // si3 — Credit overdue 45 days (cust3, F15 demo)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260320001"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260320001", CustomerId = cust3.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-75), PaymentStatus = "Credit",
                CreditDueDate = DateTime.UtcNow.AddDays(-45),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part10.Id, Quantity = 1, UnitPrice = part10.UnitPrice },
                    new() { PartId = part8.Id,  Quantity = 2, UnitPrice = part8.UnitPrice  },
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice);
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // si4 — Partial payment overdue 35 days (cust4, F15 demo)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260401001"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260401001", CustomerId = cust4.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-65), PaymentStatus = "Partial",
                CreditDueDate = DateTime.UtcNow.AddDays(-35),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part3.Id,  Quantity = 2, UnitPrice = part3.UnitPrice  },
                    new() { PartId = part6.Id,  Quantity = 3, UnitPrice = part6.UnitPrice  },
                    new() { PartId = part11.Id, Quantity = 2, UnitPrice = part11.UnitPrice },
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice);
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // si5 — Paid TODAY, subtotal > 5,000, loyalty discount (F1 daily + F16 demo #2)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260519001"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260519001", CustomerId = cust5.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow, PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part7.Id,  Quantity = 1, UnitPrice = part7.UnitPrice  }, // Rs 2,200
                    new() { PartId = part9.Id,  Quantity = 1, UnitPrice = part9.UnitPrice  }, // Rs 1,800
                    new() { PartId = part10.Id, Quantity = 1, UnitPrice = part10.UnitPrice }, // Rs 3,500
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 7,500
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // si6 — Credit overdue 70 days (cust2, F9/F15 demo)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260310002"))
        {
            var si = new SalesInvoice {
                InvoiceNumber = "INV-20260310002", CustomerId = cust2.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-100), PaymentStatus = "Credit",
                CreditDueDate = DateTime.UtcNow.AddDays(-70),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part2.Id, Quantity = 2, UnitPrice = part2.UnitPrice },
                    new() { PartId = part4.Id, Quantity = 3, UnitPrice = part4.UnitPrice },
                }
            };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice);
            si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
            si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }

        // Additional sales invoices — added individually so they are inserted even
        // if the table already existed from a prior seed run.

        // si7: cust8 (krixh.dhakal@gmail.com) — Credit, 30 days overdue (F15 demo)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260415001"))
        {
            var si7 = new SalesInvoice {
                InvoiceNumber = "INV-20260415001", CustomerId = cust8.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-55), PaymentStatus = "Credit",
                CreditDueDate = DateTime.UtcNow.AddDays(-30),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part5.Id,  Quantity = 2, UnitPrice = part5.UnitPrice  }, // Rs   900
                    new() { PartId = part6.Id,  Quantity = 3, UnitPrice = part6.UnitPrice  }, // Rs 1,650
                    new() { PartId = part12.Id, Quantity = 1, UnitPrice = part12.UnitPrice }, // Rs   720
                }
            };
            si7.Subtotal = si7.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 3,270
            si7.Discount = si7.Subtotal > 5000 ? si7.Subtotal * 0.10m : 0;
            si7.TotalAmount = si7.Subtotal - si7.Discount;
            db.SalesInvoices.Add(si7);
            await db.SaveChangesAsync();
        }

        // si8: cust9 (krixh.dhakal@gmail.com) — Credit, 35 days overdue (F15 demo)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260410001"))
        {
            var si8 = new SalesInvoice {
                InvoiceNumber = "INV-20260410001", CustomerId = cust9.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-50), PaymentStatus = "Credit",
                CreditDueDate = DateTime.UtcNow.AddDays(-35),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part8.Id,  Quantity = 2, UnitPrice = part8.UnitPrice  }, // Rs   960
                    new() { PartId = part11.Id, Quantity = 3, UnitPrice = part11.UnitPrice }, // Rs 1,140
                    new() { PartId = part4.Id,  Quantity = 5, UnitPrice = part4.UnitPrice  }, // Rs 1,750
                }
            };
            si8.Subtotal = si8.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 3,850
            si8.Discount = si8.Subtotal > 5000 ? si8.Subtotal * 0.10m : 0;
            si8.TotalAmount = si8.Subtotal - si8.Discount;
            db.SalesInvoices.Add(si8);
            await db.SaveChangesAsync();
        }

        // si9: cust6 — Paid today, subtotal > 5,000, loyalty discount (3rd F16 demo + 2nd today invoice)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260530002"))
        {
            var si9 = new SalesInvoice {
                InvoiceNumber = "INV-20260530002", CustomerId = cust6.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow, PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part7.Id, Quantity = 2, UnitPrice = part7.UnitPrice }, // Rs 4,400
                    new() { PartId = part9.Id, Quantity = 1, UnitPrice = part9.UnitPrice }, // Rs 1,800
                }
            };
            si9.Subtotal = si9.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 6,200
            si9.Discount = si9.Subtotal > 5000 ? si9.Subtotal * 0.10m : 0;
            si9.TotalAmount = si9.Subtotal - si9.Discount;
            db.SalesInvoices.Add(si9);
            await db.SaveChangesAsync();
        }

        // si10: cust7 — Paid (was credit, now settled — contrast: must NOT trigger reminder)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260501001"))
        {
            var si10 = new SalesInvoice {
                InvoiceNumber = "INV-20260501001", CustomerId = cust7.Id, StaffId = staffUser1.Id,
                Date = DateTime.UtcNow.AddDays(-29), PaymentStatus = "Paid",
                CreditDueDate = DateTime.UtcNow.AddDays(-14),
                Items = new List<SalesInvoiceItem> {
                    new() { PartId = part3.Id, Quantity = 2, UnitPrice = part3.UnitPrice }, // Rs 1,300
                    new() { PartId = part6.Id, Quantity = 2, UnitPrice = part6.UnitPrice }, // Rs 1,100
                }
            };
            si10.Subtotal = si10.Items.Sum(i => i.Quantity * i.UnitPrice); // Rs 2,400
            si10.Discount = si10.Subtotal > 5000 ? si10.Subtotal * 0.10m : 0;
            si10.TotalAmount = si10.Subtotal - si10.Discount;
            db.SalesInvoices.Add(si10);
            await db.SaveChangesAsync();
        }

        // ── 9. Appointments ─────────────────────────────────────────────────────
        if (!await db.Appointments.AnyAsync())
        {
            var veh1 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 1 CHA 1234");
            var veh2 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 2 CHA 5678");
            var veh3 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 3 PA 9012");
            var veh4 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 4 CHA 3456");
            var veh5 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 5 JA 7890");
            var veh6 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "GA 1 CHA 1111");
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
            var veh2 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 2 CHA 5678");
            var veh4 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 4 CHA 3456");
            var veh5 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 5 JA 7890");
            db.UnavailablePartRequests.AddRange(
                new UnavailablePartRequest { CustomerId = cust1.Id, VehicleId = veh2.Id, PartName = "Catalytic Converter", Description = "Need OEM cat converter for Corolla 2018", Urgency = "High",   Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-5) },
                new UnavailablePartRequest { CustomerId = cust3.Id, VehicleId = veh4.Id, PartName = "ABS Sensor (Rear)",   Description = "Rear ABS sensor for i20 2020",            Urgency = "Medium", Status = "InProgress", RequestDate = DateTime.UtcNow.AddDays(-3) },
                new UnavailablePartRequest { CustomerId = cust4.Id, VehicleId = veh5.Id, PartName = "Carburetor Jet Kit",  Description = "Performance jet kit for Pulsar 150",      Urgency = "Low",    Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-1) }
            );
            await db.SaveChangesAsync();
        }

        // ── 12. Service History ─────────────────────────────────────────────────
        if (!await db.ServiceHistories.AnyAsync())
        {
            db.ServiceHistories.AddRange(
                new ServiceHistory { CustomerId = cust1.Id, VehicleId = (await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 1 CHA 1234")).Id, ServiceType = "Oil Change",       Description = "Synthetic oil + filter replaced",          Technician = "Alice Johnson", Cost = 1200m, ServiceDate = DateTime.UtcNow.AddDays(-30), Status = "Completed" },
                new ServiceHistory { CustomerId = cust1.Id, VehicleId = (await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 2 CHA 5678")).Id, ServiceType = "Brake Service",    Description = "Front brake pads replaced",                Technician = "Bob Smith",     Cost = 2400m, ServiceDate = DateTime.UtcNow.AddDays(-15), Status = "Completed" },
                new ServiceHistory { CustomerId = cust2.Id, VehicleId = (await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 3 PA 9012")).Id,  ServiceType = "Tune-Up",          Description = "Spark plug + air filter + throttle clean", Technician = "Alice Johnson", Cost = 1800m, ServiceDate = DateTime.UtcNow.AddDays(-7),  Status = "Completed" },
                new ServiceHistory { CustomerId = cust5.Id, VehicleId = (await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "GA 1 CHA 1111")).Id, ServiceType = "Battery Replaced", Description = "12V 7Ah battery installed",                Technician = "Bob Smith",     Cost = 3800m, ServiceDate = DateTime.UtcNow.AddDays(-3),  Status = "Completed" }
            );
            await db.SaveChangesAsync();
        }

        // ── 13. Notifications ───────────────────────────────────────────────────
        // Seed general notifications only once
        if (!await db.Notifications.AnyAsync())
        {
            db.Notifications.AddRange(
                new Notification { Title = "New Part Request",   Message = "Customer Ravi Kumar requested Catalytic Converter.",  Type = "Request", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Notification { Title = "New Review",         Message = "Ravi Kumar left a 5-star review.",                   Type = "Review",  IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-28) }
            );
            await db.SaveChangesAsync();
        }

        // ── 13a. Low-Stock Notifications (5 required, idempotent per title) ────
        var lowStockSeeds = new (string Title, string Message, int HoursAgo)[]
        {
            ("Low Stock: Throttle Body Gasket",          $"Throttle Body Gasket has only 3 units remaining (reorder level: 5). Please reorder soon.",        1),
            ("Low Stock: Throttle Body Gasket (Critical)",$"CRITICAL: Throttle Body Gasket ({part13.PartCode}) stock at 3 units — below reorder threshold.", 2),
            ("Low Stock: Suspension Bush Kit",           $"Suspension Bush Kit has only 7 units in stock (reorder level: 10). Consider restocking.",          3),
            ("Low Stock: Suspension Bush Kit (Alert)",   $"Suspension Bush Kit ({part14.PartCode}) stock (7 units) is below the reorder level of 10.",        4),
            ("Low Stock: Coolant Temperature Sensor",    $"Coolant Temperature Sensor has 9 units remaining (reorder level: 12). Reorder recommended.",       5),
        };
        foreach (var (title, message, hoursAgo) in lowStockSeeds)
        {
            if (!await db.Notifications.AnyAsync(n => n.Title == title && n.Type == "LowStock"))
            {
                db.Notifications.Add(new Notification {
                    Title = title, Message = message, Type = "LowStock",
                    IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-hoursAgo)
                });
            }
        }
        await db.SaveChangesAsync();
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

    private static async Task EnsureVehicle(
        ApplicationDbContext db, Guid customerId, string vehicleNumber,
        string brand, string model, string vehicleType)
    {
        if (await db.CustomerVehicles.AnyAsync(v => v.VehicleNumber == vehicleNumber)) return;
        db.CustomerVehicles.Add(new CustomerVehicle {
            CustomerId = customerId, VehicleNumber = vehicleNumber,
            Brand = brand, Model = model, VehicleType = vehicleType
        });
        await db.SaveChangesAsync();
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

    private static async Task EnsurePurchaseInvoice(
        ApplicationDbContext db, string invoiceNumber, Guid vendorId,
        DateTime date, string notes, (Guid PartId, int Qty, decimal Cost)[] items)
    {
        if (await db.PurchaseInvoices.AnyAsync(p => p.InvoiceNumber == invoiceNumber)) return;
        var pi = new PurchaseInvoice {
            InvoiceNumber = invoiceNumber, VendorId = vendorId, Date = date, Notes = notes,
            Items = items.Select(i => new PurchaseInvoiceItem { PartId = i.PartId, Quantity = i.Qty, UnitCost = i.Cost }).ToList()
        };
        pi.TotalAmount = pi.Items.Sum(i => i.Quantity * i.UnitCost);
        db.PurchaseInvoices.Add(pi);
        await db.SaveChangesAsync();
    }
}
