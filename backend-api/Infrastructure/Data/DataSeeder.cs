using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Data;

public static class DataSeeder
{
    private static DateTime Utc(int year, int month, int day) =>
        DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db          = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // ── 1. Roles ─────────────────────────────────────────────────────────────
        foreach (var role in new[] { "Admin", "Staff", "Customer" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        // ── 2. Staff users (8 total) ──────────────────────────────────────────────
        var staffUser1 = await EnsureUser(userManager, "staff1@vparts.com", "Staff@123", "Alice Johnson",  "Staff", "Sales Associate");
        var staffUser2 = await EnsureUser(userManager, "staff2@vparts.com", "Staff@123", "Bob Smith",      "Staff", "Parts Manager");
        var staffUser3 = await EnsureUser(userManager, "staff3@vparts.com", "Staff@123", "Carol Thapa",    "Staff", "Service Advisor");
        var staffUser4 = await EnsureUser(userManager, "staff4@vparts.com", "Staff@123", "David Rai",      "Staff", "Mechanic");
        var staffUser5 = await EnsureUser(userManager, "staff5@vparts.com", "Staff@123", "Emma Shrestha",  "Staff", "Service Advisor");
        var staffUser6 = await EnsureUser(userManager, "staff6@vparts.com", "Staff@123", "Frank Lama",     "Staff", "Mechanic");
        var staffUser7 = await EnsureUser(userManager, "staff7@vparts.com", "Staff@123", "Grace Pandey",   "Staff", "Admin Assistant");
        var staffUser8 = await EnsureUser(userManager, "staff8@vparts.com", "Staff@123", "Henry Malla",    "Staff", "Parts Manager");

        // ── 3. Customer users (20 total) ──────────────────────────────────────────
        var custUser1  = await EnsureCustomerUser(userManager, "customer1@vparts.com",  "Ravi Kumar",          "9841000001");
        var custUser2  = await EnsureCustomerUser(userManager, "customer2@vparts.com",  "Sita Sharma",         "9841000002");
        var custUser3  = await EnsureCustomerUser(userManager, "customer3@vparts.com",  "Hari Thapa",          "9841000003");
        var custUser4  = await EnsureCustomerUser(userManager, "customer4@vparts.com",  "Gita Rai",            "9841000004");
        var custUser5  = await EnsureCustomerUser(userManager, "customer5@vparts.com",  "Bikash Gurung",       "9841000005");
        var custUser6  = await EnsureCustomerUser(userManager, "customer6@vparts.com",  "Ram Bahadur Kshetri", "9841000006");
        var custUser7  = await EnsureCustomerUser(userManager, "customer7@vparts.com",  "Sarita Adhikari",     "9841000007");
        var custUser10 = await EnsureCustomerUser(userManager, "customer10@vparts.com", "Anita Poudel",        "9841000010");

        // Credit-demo customers share email so must use FindByName
        ApplicationUser? custUser8 = await userManager.FindByNameAsync("creditdemo1@vparts.com");
        ApplicationUser? custUser9 = await userManager.FindByNameAsync("creditdemo2@vparts.com");
        if (custUser8 == null) { custUser8 = new ApplicationUser { UserName = "creditdemo1@vparts.com", Email = "krixh.dhakal@gmail.com", FullName = "Bijaya Magar",  PhoneNumber = "9841000008", EmailConfirmed = true }; await userManager.CreateAsync(custUser8, "Cust@123"); await userManager.AddToRoleAsync(custUser8, "Customer"); }
        if (custUser9 == null) { custUser9 = new ApplicationUser { UserName = "creditdemo2@vparts.com", Email = "krixh.dhakal@gmail.com", FullName = "Suresh Tamang", PhoneNumber = "9841000009", EmailConfirmed = true }; await userManager.CreateAsync(custUser9, "Cust@123"); await userManager.AddToRoleAsync(custUser9, "Customer"); }

        var custUser11 = await EnsureCustomerUser(userManager, "customer11@vparts.com", "Prakash Basnet",      "9841000011");
        var custUser12 = await EnsureCustomerUser(userManager, "customer12@vparts.com", "Sunita Bhandari",     "9841000012");
        var custUser13 = await EnsureCustomerUser(userManager, "customer13@vparts.com", "Deepak Joshi",        "9841000013");
        var custUser14 = await EnsureCustomerUser(userManager, "customer14@vparts.com", "Kamala Devi",         "9841000014");
        var custUser15 = await EnsureCustomerUser(userManager, "customer15@vparts.com", "Narayan Paudel",      "9841000015");
        var custUser16 = await EnsureCustomerUser(userManager, "customer16@vparts.com", "Reshma Karki",        "9841000016");
        var custUser17 = await EnsureCustomerUser(userManager, "customer17@vparts.com", "Sujan Maharjan",      "9841000017");
        var custUser18 = await EnsureCustomerUser(userManager, "customer18@vparts.com", "Puja Lama",           "9841000018");
        var custUser19 = await EnsureCustomerUser(userManager, "customer19@vparts.com", "Mohan Khadka",        "9841000019");
        var custUser20 = await EnsureCustomerUser(userManager, "customer20@vparts.com", "Binita Shrestha",     "9841000020");

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
        var cust11 = await EnsureCustomer(db, custUser11.Id, "Kathmandu, Patan Dhoka");
        var cust12 = await EnsureCustomer(db, custUser12.Id, "Lalitpur, Sanepa");
        var cust13 = await EnsureCustomer(db, custUser13.Id, "Kathmandu, Balaju");
        var cust14 = await EnsureCustomer(db, custUser14.Id, "Bhaktapur, Banepa");
        var cust15 = await EnsureCustomer(db, custUser15.Id, "Pokhara, Damside");
        var cust16 = await EnsureCustomer(db, custUser16.Id, "Kathmandu, New Road");
        var cust17 = await EnsureCustomer(db, custUser17.Id, "Lalitpur, Pulchowk");
        var cust18 = await EnsureCustomer(db, custUser18.Id, "Kathmandu, Maharajgunj");
        var cust19 = await EnsureCustomer(db, custUser19.Id, "Pokhara, Prithvi Chowk");
        var cust20 = await EnsureCustomer(db, custUser20.Id, "Chitwan, Narayangarh");

        // ── 4. Customer Vehicles (25+ total) ──────────────────────────────────────
        await EnsureVehicle(db, cust1.Id,  "BA 1 CHA 1234",  "Honda",    "CB Shine",       "Motorcycle");
        await EnsureVehicle(db, cust1.Id,  "BA 2 CHA 5678",  "Toyota",   "Corolla",        "Car");
        await EnsureVehicle(db, cust2.Id,  "BA 3 PA 9012",   "Yamaha",   "FZ-S",           "Motorcycle");
        await EnsureVehicle(db, cust3.Id,  "BA 4 CHA 3456",  "Hyundai",  "i20",            "Car");
        await EnsureVehicle(db, cust4.Id,  "BA 5 JA 7890",   "Bajaj",    "Pulsar 150",     "Motorcycle");
        await EnsureVehicle(db, cust5.Id,  "GA 1 CHA 1111",  "Suzuki",   "Swift",          "Car");
        await EnsureVehicle(db, cust6.Id,  "BA 7 CHA 2222",  "Mahindra", "Thar",           "SUV");
        await EnsureVehicle(db, cust7.Id,  "BA 8 JA 3333",   "KTM",      "Duke 200",       "Motorcycle");
        await EnsureVehicle(db, cust8.Id,  "GA 2 CHA 4444",  "Honda",    "Activa 6G",      "Scooter");
        await EnsureVehicle(db, cust9.Id,  "GA 3 PA 5555",   "Hero",     "Splendor Plus",  "Motorcycle");
        await EnsureVehicle(db, cust10.Id, "BA 9 CHA 6666",  "Tata",     "Nexon",          "SUV");
        await EnsureVehicle(db, cust11.Id, "BA 10 PA 1010",  "Honda",    "City",           "Car");
        await EnsureVehicle(db, cust12.Id, "BA 11 CHA 2020", "Yamaha",   "R15 V4",         "Motorcycle");
        await EnsureVehicle(db, cust13.Id, "BA 12 JA 3030",  "Toyota",   "Fortuner",       "SUV");
        await EnsureVehicle(db, cust14.Id, "GA 4 CHA 4040",  "Hyundai",  "Creta",          "SUV");
        await EnsureVehicle(db, cust15.Id, "GA 5 PA 5050",   "Bajaj",    "Avenger 220",    "Motorcycle");
        await EnsureVehicle(db, cust16.Id, "BA 13 CHA 6060", "Suzuki",   "Ertiga",         "Car");
        await EnsureVehicle(db, cust17.Id, "BA 14 JA 7070",  "Honda",    "Dio",            "Scooter");
        await EnsureVehicle(db, cust18.Id, "BA 15 CHA 8080", "Mahindra", "Scorpio",        "SUV");
        await EnsureVehicle(db, cust19.Id, "GA 6 CHA 9090",  "KTM",      "390 Adventure",  "Motorcycle");
        await EnsureVehicle(db, cust20.Id, "BA 16 PA 1001",  "Tata",     "Harrier",        "SUV");
        await EnsureVehicle(db, cust2.Id,  "BA 17 CHA 2002", "Toyota",   "Hilux",          "SUV");
        await EnsureVehicle(db, cust5.Id,  "GA 7 JA 3003",   "Honda",    "CBR 150R",       "Motorcycle");
        await EnsureVehicle(db, cust6.Id,  "BA 18 CHA 4004", "Hyundai",  "Tucson",         "Car");
        await EnsureVehicle(db, cust13.Id, "BA 19 CHA 5005", "Ford",     "Endeavour",      "SUV");

        // ── 5. Vendors (15 total) ─────────────────────────────────────────────────
        var vend1  = await EnsureVendor(db, "AutoParts Nepal Pvt. Ltd.",  "Ramesh Shrestha",    "9801111111", "autoparts@nepal.com",        "Kathmandu, New Baneshwor");
        var vend2  = await EnsureVendor(db, "Himalayan Motors Supply",    "Sunita Karki",       "9802222222", "himalayan@motors.com",       "Lalitpur, Kumaripati");
        var vend3  = await EnsureVendor(db, "Everest Auto Components",    "Dipak Tamang",       "9803333333", "everest@autocomp.com",       "Bhaktapur, Katunje");
        var vend4  = await EnsureVendor(db, "Nepal Motor Parts House",    "Laxman Bhattarai",   "9804444444", "nepal.motorparts@gmail.com", "Kathmandu, Kalanki");
        var vend5  = await EnsureVendor(db, "Kathmandu Auto Supplies",    "Pratima Maharjan",   "9805555555", "ktm.autosupplies@yahoo.com", "Lalitpur, Jawalakhel");
        var vend6  = await EnsureVendor(db, "Pokhara Vehicle Parts",      "Naresh Gurung",      "9806666666", "pokhara.vparts@gmail.com",   "Pokhara, Prithvi Chowk");
        var vend7  = await EnsureVendor(db, "Birgunj Parts Depot",        "Ramesh Yadav",       "9807777777", "birgunj.parts@gmail.com",    "Birgunj, Ghantaghar");
        var vend8  = await EnsureVendor(db, "Butwal Mechanics Supply",    "Indira Thapa Magar", "9808888888", "butwal.mech@nepal.com",      "Butwal, Traffic Chowk");
        var vend9  = await EnsureVendor(db, "Janakpur Auto Center",       "Sanjay Shah",        "9809999999", "janakpur.auto@yahoo.com",    "Janakpur, Station Road");
        var vend10 = await EnsureVendor(db, "Dharan Auto Works",          "Kamala Rai",         "9800000001", "dharan.autoworks@gmail.com", "Dharan, BP Chowk");
        var vend11 = await EnsureVendor(db, "Biratnagar Parts House",     "Suman Limbu",        "9800000002", "biratnagar.parts@gmail.com", "Biratnagar, Main Road");
        var vend12 = await EnsureVendor(db, "Hetauda Motor Spares",       "Priya Tamang",       "9800000003", "hetauda.motors@yahoo.com",   "Hetauda, Bus Park");
        var vend13 = await EnsureVendor(db, "Palpa Auto Enterprises",     "Kiran Shrestha",     "9800000004", "palpa.auto@gmail.com",       "Palpa, Tansen");
        var vend14 = await EnsureVendor(db, "Dang Vehicle Components",    "Ritu Thapa",         "9800000005", "dang.vehiclecomp@gmail.com", "Dang, Ghorahi");
        var vend15 = await EnsureVendor(db, "Baglung Parts Traders",      "Bhim Gurung",        "9800000006", "baglung.parts@nepal.com",    "Baglung, Bazar");

        // ── 6. Parts (25 total) ───────────────────────────────────────────────────
        var part1  = await EnsurePart(db, "Engine Oil Filter",          "ENG-OIL-001", "Engine",     "High-quality oil filter for 4-stroke engines",         850m,  45, 10, vend1.Id);
        var part2  = await EnsurePart(db, "Brake Pad Set (Front)",      "BRK-PAD-001", "Brakes",     "Front disc brake pads, fits most Japanese cars",       1200m, 30,  8, vend1.Id);
        var part3  = await EnsurePart(db, "Air Filter",                 "AIR-FLT-001", "Engine",     "Dry-type air filter for motorcycles and small cars",    650m,  60, 15, vend2.Id);
        var part4  = await EnsurePart(db, "Spark Plug (NGK)",           "SPK-PLG-001", "Ignition",   "NGK standard spark plug, universal fit",                350m,  80, 20, vend2.Id);
        var part5  = await EnsurePart(db, "Clutch Cable",               "CLT-CBL-001", "Drivetrain", "Replacement clutch cable for motorcycles",              450m,  25,  5, vend3.Id);
        var part6  = await EnsurePart(db, "Radiator Coolant 1L",        "RAD-CLT-001", "Cooling",    "Concentrated coolant, mix 50/50 with water",            550m,  40, 10, vend1.Id);
        var part7  = await EnsurePart(db, "Timing Belt",                "TIM-BLT-001", "Engine",     "OEM-spec timing belt for 1.3-1.6L engines",            2200m, 15,  4, vend3.Id);
        var part8  = await EnsurePart(db, "Headlight Bulb H4",          "LGT-BLB-001", "Electrical", "H4 halogen bulb 60/55W, pair",                          480m,  50, 12, vend2.Id);
        var part9  = await EnsurePart(db, "Wheel Bearing (Front)",      "WHL-BRG-001", "Suspension", "Front wheel bearing kit, fits Toyota/Honda",           1800m,  20,  5, vend1.Id);
        var part10 = await EnsurePart(db, "Battery 12V 7Ah",            "BAT-12V-001", "Electrical", "Maintenance-free sealed lead-acid battery",            3500m,  12,  3, vend3.Id);
        var part11 = await EnsurePart(db, "Wiper Blade 18\"",           "WPR-BLD-001", "Body",       "18-inch frameless wiper blade",                         380m,  35,  8, vend2.Id);
        var part12 = await EnsurePart(db, "Fuel Filter",                "FUL-FLT-001", "Fuel",       "Inline fuel filter for petrol engines",                 720m,  28,  7, vend1.Id);
        // Low-stock parts (below reorder level — trigger alert notifications)
        var part13 = await EnsurePart(db, "Throttle Body Gasket",       "THR-GSK-001", "Engine",     "OEM throttle body gasket",                              290m,   3,  5, vend3.Id);
        var part14 = await EnsurePart(db, "Suspension Bush Kit",        "SUS-BSH-001", "Suspension", "Front suspension bush set, polyurethane",              1650m,   7, 10, vend4.Id);
        var part15 = await EnsurePart(db, "Coolant Temperature Sensor", "CLT-SEN-001", "Cooling",    "OEM coolant temp sensor, fits Honda/Toyota/Hyundai",  2100m,   9, 12, vend5.Id);
        // New parts
        var part16 = await EnsurePart(db, "Brake Disc Rotor",           "BRK-DSC-001", "Brakes",     "Ventilated front disc rotor, 256mm diameter",          2800m, 18,  5, vend4.Id);
        var part17 = await EnsurePart(db, "Chain Sprocket Kit",         "DRV-SPK-001", "Drivetrain", "Chain and sprocket set for 150cc-200cc motorcycles",   1950m, 22,  6, vend5.Id);
        var part18 = await EnsurePart(db, "Alternator Belt",            "ALT-BLT-001", "Electrical", "V-ribbed alternator belt, fits 1.3-2.0L engines",       780m,  32,  8, vend6.Id);
        var part19 = await EnsurePart(db, "Shock Absorber (Rear)",      "SHK-ABS-001", "Suspension", "Gas-charged rear shock absorber",                      3200m,  10,  3, vend7.Id);
        var part20 = await EnsurePart(db, "Thermostat Assembly",        "THR-ASM-001", "Cooling",    "Engine thermostat with housing, OEM spec",              1450m,  14,  4, vend6.Id);
        // Low-stock new parts
        var part21 = await EnsurePart(db, "Ignition Coil",              "IGN-COL-001", "Ignition",   "Direct ignition coil, fits Honda/Yamaha 150cc+",       2600m,   2,  5, vend8.Id);
        var part22 = await EnsurePart(db, "Power Steering Fluid 1L",    "PSF-FLD-001", "Steering",   "OEM-grade power steering fluid",                        420m,  55, 12, vend9.Id);
        var part23 = await EnsurePart(db, "Exhaust Gasket Set",         "EXH-GSK-001", "Engine",     "Complete exhaust manifold gasket kit",                   560m,   4,  6, vend10.Id);
        var part24 = await EnsurePart(db, "Cabin Air Filter",           "CAB-FLT-001", "Body",       "Interior cabin air filter, removes dust and pollen",    490m,  38,  9, vend11.Id);
        var part25 = await EnsurePart(db, "Gear Oil 90W 1L",            "GER-OIL-001", "Drivetrain", "GL-4 gear oil for manual transmissions",                680m,  42, 10, vend12.Id);

        // ── 7. Purchase Invoices (30+ spread across 2024-2026) ────────────────────

        // 2024
        await EnsurePurchaseInvoice(db, "PUR-20240115001", vend1.Id, Utc(2024,1,15),  "2024 Q1 initial stock",           new[] { (part1.Id, 30, 680m),  (part2.Id, 20, 940m),   (part6.Id, 25, 415m) });
        await EnsurePurchaseInvoice(db, "PUR-20240220001", vend2.Id, Utc(2024,2,20),  "Ignition and engine parts",       new[] { (part3.Id, 40, 490m),  (part4.Id, 60, 265m),   (part8.Id, 30, 350m) });
        await EnsurePurchaseInvoice(db, "PUR-20240410001", vend3.Id, Utc(2024,4,10),  "Heavy parts Q2 2024",             new[] { (part7.Id, 10, 1680m), (part9.Id, 15, 1380m),  (part10.Id, 8, 2750m) });
        await EnsurePurchaseInvoice(db, "PUR-20240618001", vend4.Id, Utc(2024,6,18),  "Mid-year suspension restock",     new[] { (part14.Id,15, 1220m), (part16.Id,12, 2200m),  (part19.Id, 8, 2500m) });
        await EnsurePurchaseInvoice(db, "PUR-20240812001", vend5.Id, Utc(2024,8,12),  "August cooling and drivetrain",   new[] { (part5.Id, 20, 335m),  (part17.Id,18, 1520m),  (part20.Id,12, 1120m) });
        await EnsurePurchaseInvoice(db, "PUR-20241005001", vend6.Id, Utc(2024,10,5),  "October comprehensive stock",     new[] { (part11.Id,30, 280m),  (part12.Id,20, 550m),   (part18.Id,25, 600m),  (part22.Id,40, 320m) });
        await EnsurePurchaseInvoice(db, "PUR-20241215001", vend7.Id, Utc(2024,12,15), "Year-end electrical parts",       new[] { (part8.Id, 35, 345m),  (part10.Id,10, 2720m),  (part21.Id, 6, 2000m), (part23.Id, 8, 420m) });

        // 2025
        await EnsurePurchaseInvoice(db, "PUR-20250118001", vend1.Id, Utc(2025,1,18),  "2025 Q1 opening order",           new[] { (part1.Id, 35, 690m),  (part2.Id, 25, 950m),   (part4.Id, 70, 268m) });
        await EnsurePurchaseInvoice(db, "PUR-20250305001", vend2.Id, Utc(2025,3,5),   "Spring engine components",        new[] { (part3.Id, 45, 495m),  (part7.Id, 12, 1690m),  (part13.Id,10, 215m) });
        await EnsurePurchaseInvoice(db, "PUR-20250422001", vend3.Id, Utc(2025,4,22),  "April body and cooling",          new[] { (part6.Id, 30, 420m),  (part11.Id,40, 285m),   (part15.Id,15, 1610m) });
        await EnsurePurchaseInvoice(db, "PUR-20250710001", vend4.Id, Utc(2025,7,10),  "Q3 suspension and brakes",        new[] { (part9.Id, 18, 1390m), (part14.Id,20, 1230m),  (part16.Id,15, 2210m) });
        await EnsurePurchaseInvoice(db, "PUR-20250915001", vend5.Id, Utc(2025,9,15),  "September drivetrain bulk",       new[] { (part5.Id, 25, 340m),  (part17.Id,22, 1530m),  (part25.Id,50, 530m) });
        await EnsurePurchaseInvoice(db, "PUR-20251120001", vend8.Id, Utc(2025,11,20), "November mixed components",       new[] { (part19.Id,10, 2510m), (part20.Id,15, 1130m),  (part24.Id,45, 380m) });
        await EnsurePurchaseInvoice(db, "PUR-20251205001", vend9.Id, Utc(2025,12,5),  "Year-end stock buffer",           new[] { (part12.Id,25, 555m),  (part18.Id,30, 610m),   (part22.Id,50, 325m) });

        // 2026 (original 3 preserved + additional)
        if (!await db.PurchaseInvoices.AnyAsync(p => p.InvoiceNumber == "PUR-20260101001"))
        {
            var pi = new PurchaseInvoice {
                InvoiceNumber = "PUR-20260101001", VendorId = vend1.Id, Date = Utc(2026,1,15), Notes = "Initial stock purchase",
                Items = new List<PurchaseInvoiceItem> {
                    new() { PartId = part1.Id, Quantity = 20, UnitCost = 700m },
                    new() { PartId = part2.Id, Quantity = 15, UnitCost = 950m },
                    new() { PartId = part6.Id, Quantity = 20, UnitCost = 420m },
                    new() { PartId = part9.Id, Quantity = 10, UnitCost = 1400m },
                }
            };
            pi.TotalAmount = pi.Items.Sum(i => i.Quantity * i.UnitCost);
            db.PurchaseInvoices.Add(pi); await db.SaveChangesAsync();
        }
        await EnsurePurchaseInvoice(db, "PUR-20260201001", vend2.Id, Utc(2026,2,10),  "Restocking order",                new[] { (part3.Id, 30, 500m),  (part4.Id, 40, 270m),   (part8.Id, 25, 360m),  (part11.Id,20, 290m) });
        await EnsurePurchaseInvoice(db, "PUR-20260301001", vend3.Id, Utc(2026,3,8),   "Urgent restock",                  new[] { (part5.Id, 12, 340m),  (part7.Id,  8, 1700m),  (part10.Id, 6, 2800m), (part12.Id,15, 560m) });
        await EnsurePurchaseInvoice(db, "PUR-20260405001", vend6.Id, Utc(2026,4,5),   "April fuel and cooling restock",  new[] { (part6.Id, 30, 415m),  (part12.Id,20, 550m) });
        await EnsurePurchaseInvoice(db, "PUR-20260415001", vend3.Id, Utc(2026,4,15),  "April heavy components order",    new[] { (part10.Id, 8, 2750m), (part7.Id,  6, 1680m),  (part14.Id, 8, 1230m) });
        await EnsurePurchaseInvoice(db, "PUR-20260425001", vend7.Id, Utc(2026,4,25),  "April miscellaneous stock",       new[] { (part11.Id,25, 280m),  (part13.Id, 5, 210m),   (part15.Id, 7, 1580m) });
        await EnsurePurchaseInvoice(db, "PUR-20260502001", vend4.Id, Utc(2026,5,2),   "Suspension and cooling parts",    new[] { (part14.Id,12, 1250m), (part15.Id,10, 1600m) });
        await EnsurePurchaseInvoice(db, "PUR-20260508001", vend1.Id, Utc(2026,5,8),   "Engine parts bulk order",         new[] { (part1.Id, 25, 680m),  (part3.Id, 20, 490m),   (part7.Id, 10, 1700m) });
        await EnsurePurchaseInvoice(db, "PUR-20260512001", vend5.Id, Utc(2026,5,12),  "Electrical and ignition spares",  new[] { (part4.Id, 50, 260m),  (part8.Id, 30, 350m) });
        await EnsurePurchaseInvoice(db, "PUR-20260518001", vend2.Id, Utc(2026,5,18),  "Brake and drivetrain components", new[] { (part2.Id, 20, 940m),  (part5.Id, 15, 335m),   (part9.Id, 12, 1400m) });
        await EnsurePurchaseInvoice(db, "PUR-20260525001", vend13.Id,Utc(2026,5,25),  "Late May new parts order",        new[] { (part21.Id, 8, 2010m), (part23.Id,10, 425m),   (part24.Id,30, 382m) });

        // ── 8. Sales Invoices (35+ across 2024-2026) ──────────────────────────────

        // 2024 — gives yearly report data
        await EnsureSalesInvoice(db, "INV-20240210001", cust11.Id, staffUser1.Id, Utc(2024,2,10),  "Paid",    null,
            new[] { (part1.Id,2,part1.UnitPrice),  (part3.Id,2,part3.UnitPrice) });                                   // 3,000
        await EnsureSalesInvoice(db, "INV-20240315001", cust12.Id, staffUser2.Id, Utc(2024,3,15),  "Paid",    null,
            new[] { (part7.Id,2,part7.UnitPrice),  (part9.Id,1,part9.UnitPrice),  (part2.Id,1,part2.UnitPrice) });    // 7,400 → discount
        await EnsureSalesInvoice(db, "INV-20240420001", cust2.Id,  staffUser1.Id, Utc(2024,4,20),  "Paid",    null,
            new[] { (part10.Id,1,part10.UnitPrice),(part8.Id,2,part8.UnitPrice) });                                    // 4,460
        await EnsureSalesInvoice(db, "INV-20240520001", cust13.Id, staffUser3.Id, Utc(2024,5,20),  "Paid",    null,
            new[] { (part4.Id,4,part4.UnitPrice),  (part5.Id,2,part5.UnitPrice),  (part6.Id,2,part6.UnitPrice) });    // 3,500
        await EnsureSalesInvoice(db, "INV-20240625001", cust5.Id,  staffUser1.Id, Utc(2024,6,25),  "Paid",    null,
            new[] { (part16.Id,1,part16.UnitPrice),(part9.Id,1,part9.UnitPrice),  (part2.Id,2,part2.UnitPrice) });    // 7,600 → discount
        await EnsureSalesInvoice(db, "INV-20240720001", cust14.Id, staffUser2.Id, Utc(2024,7,20),  "Paid",    null,
            new[] { (part19.Id,1,part19.UnitPrice),(part17.Id,1,part17.UnitPrice) });                                  // 5,150 → discount
        await EnsureSalesInvoice(db, "INV-20240815001", cust6.Id,  staffUser1.Id, Utc(2024,8,15),  "Paid",    null,
            new[] { (part7.Id,1,part7.UnitPrice),  (part20.Id,1,part20.UnitPrice) });                                  // 3,650
        await EnsureSalesInvoice(db, "INV-20240918001", cust15.Id, staffUser3.Id, Utc(2024,9,18),  "Paid",    null,
            new[] { (part4.Id,6,part4.UnitPrice),  (part11.Id,3,part11.UnitPrice),(part22.Id,2,part22.UnitPrice) });  // 3,294
        await EnsureSalesInvoice(db, "INV-20241010001", cust3.Id,  staffUser2.Id, Utc(2024,10,10), "Paid",    null,
            new[] { (part10.Id,1,part10.UnitPrice),(part9.Id,1,part9.UnitPrice),  (part2.Id,1,part2.UnitPrice) });    // 6,500 → discount
        await EnsureSalesInvoice(db, "INV-20241120001", cust16.Id, staffUser1.Id, Utc(2024,11,20), "Paid",    null,
            new[] { (part1.Id,3,part1.UnitPrice),  (part12.Id,2,part12.UnitPrice) });                                  // 3,990
        await EnsureSalesInvoice(db, "INV-20241205001", cust2.Id,  staffUser1.Id, Utc(2024,12,5),  "Paid",    null,
            new[] { (part16.Id,1,part16.UnitPrice),(part19.Id,1,part19.UnitPrice),(part10.Id,1,part10.UnitPrice) });  // 9,500 → discount

        // 2025 — gives yearly report data
        await EnsureSalesInvoice(db, "INV-20250112001", cust17.Id, staffUser1.Id, Utc(2025,1,12),  "Paid",    null,
            new[] { (part3.Id,2,part3.UnitPrice),  (part4.Id,4,part4.UnitPrice),  (part8.Id,2,part8.UnitPrice) });    // 3,560
        await EnsureSalesInvoice(db, "INV-20250218001", cust18.Id, staffUser2.Id, Utc(2025,2,18),  "Paid",    null,
            new[] { (part7.Id,2,part7.UnitPrice),  (part9.Id,2,part9.UnitPrice) });                                    // 8,000 → discount
        await EnsureSalesInvoice(db, "INV-20250310001", cust5.Id,  staffUser3.Id, Utc(2025,3,10),  "Paid",    null,
            new[] { (part17.Id,2,part17.UnitPrice),(part5.Id,2,part5.UnitPrice) });                                    // 4,800
        await EnsureSalesInvoice(db, "INV-20250415001", cust19.Id, staffUser1.Id, Utc(2025,4,15),  "Paid",    null,
            new[] { (part19.Id,1,part19.UnitPrice),(part16.Id,1,part16.UnitPrice) });                                  // 6,000 → discount
        await EnsureSalesInvoice(db, "INV-20250520001", cust2.Id,  staffUser2.Id, Utc(2025,5,20),  "Paid",    null,
            new[] { (part10.Id,2,part10.UnitPrice),(part9.Id,1,part9.UnitPrice) });                                    // 8,800 → discount
        await EnsureSalesInvoice(db, "INV-20250618001", cust20.Id, staffUser1.Id, Utc(2025,6,18),  "Paid",    null,
            new[] { (part2.Id,2,part2.UnitPrice),  (part11.Id,3,part11.UnitPrice),(part12.Id,2,part12.UnitPrice) });  // 4,980
        await EnsureSalesInvoice(db, "INV-20250720001", cust11.Id, staffUser3.Id, Utc(2025,7,20),  "Paid",    null,
            new[] { (part7.Id,1,part7.UnitPrice),  (part20.Id,2,part20.UnitPrice),(part6.Id,3,part6.UnitPrice) });    // 6,350 → discount
        await EnsureSalesInvoice(db, "INV-20250812001", cust12.Id, staffUser1.Id, Utc(2025,8,12),  "Paid",    null,
            new[] { (part1.Id,4,part1.UnitPrice),  (part3.Id,3,part3.UnitPrice) });                                    // 5,350 → discount
        await EnsureSalesInvoice(db, "INV-20250918001", cust13.Id, staffUser2.Id, Utc(2025,9,18),  "Paid",    null,
            new[] { (part16.Id,2,part16.UnitPrice),(part9.Id,1,part9.UnitPrice) });                                    // 7,400 → discount
        await EnsureSalesInvoice(db, "INV-20251015001", cust6.Id,  staffUser3.Id, Utc(2025,10,15), "Paid",    null,
            new[] { (part7.Id,2,part7.UnitPrice),  (part2.Id,2,part2.UnitPrice) });                                    // 6,800 → discount
        await EnsureSalesInvoice(db, "INV-20251115001", cust3.Id,  staffUser1.Id, Utc(2025,11,15), "Paid",    null,
            new[] { (part19.Id,1,part19.UnitPrice),(part17.Id,1,part17.UnitPrice),(part5.Id,2,part5.UnitPrice) });    // 7,050 → discount
        await EnsureSalesInvoice(db, "INV-20251210001", cust14.Id, staffUser2.Id, Utc(2025,12,10), "Paid",    null,
            new[] { (part10.Id,1,part10.UnitPrice),(part16.Id,1,part16.UnitPrice) });                                  // 6,300 → discount

        // 2026 — original 10 invoices (individually guarded)
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260310001"))
        {
            var si = new SalesInvoice { InvoiceNumber = "INV-20260310001", CustomerId = cust1.Id, StaffId = staffUser1.Id, Date = Utc(2026,3,10), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> { new() { PartId = part1.Id, Quantity = 2, UnitPrice = part1.UnitPrice }, new() { PartId = part4.Id, Quantity = 4, UnitPrice = part4.UnitPrice } } };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0; si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260315001"))
        {
            var si = new SalesInvoice { InvoiceNumber = "INV-20260315001", CustomerId = cust2.Id, StaffId = staffUser1.Id, Date = Utc(2026,3,15), PaymentStatus = "Paid",
                Items = new List<SalesInvoiceItem> { new() { PartId = part7.Id, Quantity = 2, UnitPrice = part7.UnitPrice }, new() { PartId = part9.Id, Quantity = 1, UnitPrice = part9.UnitPrice }, new() { PartId = part2.Id, Quantity = 1, UnitPrice = part2.UnitPrice } } };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0; si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260320001"))
        {
            var si = new SalesInvoice { InvoiceNumber = "INV-20260320001", CustomerId = cust3.Id, StaffId = staffUser1.Id, Date = Utc(2026,3,20), PaymentStatus = "Credit", CreditDueDate = Utc(2026,4,5),
                Items = new List<SalesInvoiceItem> { new() { PartId = part10.Id, Quantity = 1, UnitPrice = part10.UnitPrice }, new() { PartId = part8.Id, Quantity = 2, UnitPrice = part8.UnitPrice } } };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0; si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260401001"))
        {
            var si = new SalesInvoice { InvoiceNumber = "INV-20260401001", CustomerId = cust4.Id, StaffId = staffUser1.Id, Date = Utc(2026,4,1), PaymentStatus = "Partial", CreditDueDate = Utc(2026,4,25),
                Items = new List<SalesInvoiceItem> { new() { PartId = part3.Id, Quantity = 2, UnitPrice = part3.UnitPrice }, new() { PartId = part6.Id, Quantity = 3, UnitPrice = part6.UnitPrice }, new() { PartId = part11.Id, Quantity = 2, UnitPrice = part11.UnitPrice } } };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0; si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }
        if (!await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == "INV-20260310002"))
        {
            var si = new SalesInvoice { InvoiceNumber = "INV-20260310002", CustomerId = cust2.Id, StaffId = staffUser1.Id, Date = Utc(2026,3,10), PaymentStatus = "Credit", CreditDueDate = Utc(2026,3,25),
                Items = new List<SalesInvoiceItem> { new() { PartId = part2.Id, Quantity = 2, UnitPrice = part2.UnitPrice }, new() { PartId = part4.Id, Quantity = 3, UnitPrice = part4.UnitPrice } } };
            si.Subtotal = si.Items.Sum(i => i.Quantity * i.UnitPrice); si.Discount = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0; si.TotalAmount = si.Subtotal - si.Discount;
            db.SalesInvoices.Add(si); await db.SaveChangesAsync();
        }
        // May 2026 invoices (monthly report) + today's (daily report)
        await EnsureSalesInvoice(db, "INV-20260415001", cust8.Id,  staffUser1.Id, Utc(2026,4,15),  "Credit",  Utc(2026,4,30),
            new[] { (part5.Id,2,part5.UnitPrice),  (part6.Id,3,part6.UnitPrice),  (part12.Id,1,part12.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260410001", cust9.Id,  staffUser1.Id, Utc(2026,4,10),  "Credit",  Utc(2026,4,25),
            new[] { (part8.Id,2,part8.UnitPrice),  (part11.Id,3,part11.UnitPrice),(part4.Id,5,part4.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260501001", cust7.Id,  staffUser1.Id, Utc(2026,5,1),   "Paid",    null,
            new[] { (part3.Id,2,part3.UnitPrice),  (part6.Id,2,part6.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260505001", cust15.Id, staffUser2.Id, Utc(2026,5,5),   "Paid",    null,
            new[] { (part1.Id,3,part1.UnitPrice),  (part4.Id,5,part4.UnitPrice),  (part3.Id,2,part3.UnitPrice) });    // 5,600 → discount
        await EnsureSalesInvoice(db, "INV-20260507001", cust11.Id, staffUser3.Id, Utc(2026,5,7),   "Paid",    null,
            new[] { (part16.Id,1,part16.UnitPrice),(part17.Id,1,part17.UnitPrice) });                                  // 4,750
        await EnsureSalesInvoice(db, "INV-20260510001", cust12.Id, staffUser1.Id, Utc(2026,5,10),  "Paid",    null,
            new[] { (part10.Id,1,part10.UnitPrice),(part9.Id,1,part9.UnitPrice),  (part2.Id,1,part2.UnitPrice) });    // 6,500 → discount
        await EnsureSalesInvoice(db, "INV-20260512001", cust20.Id, staffUser2.Id, Utc(2026,5,12),  "Credit",  Utc(2026,6,12),
            new[] { (part19.Id,1,part19.UnitPrice),(part8.Id,2,part8.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260515001", cust13.Id, staffUser3.Id, Utc(2026,5,15),  "Paid",    null,
            new[] { (part7.Id,1,part7.UnitPrice),  (part20.Id,1,part20.UnitPrice),(part6.Id,2,part6.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260519001", cust5.Id,  staffUser1.Id, Utc(2026,5,19),  "Paid",    null,
            new[] { (part7.Id,1,part7.UnitPrice),  (part9.Id,1,part9.UnitPrice),  (part10.Id,1,part10.UnitPrice) }); // 7,500 → discount
        await EnsureSalesInvoice(db, "INV-20260520001", cust17.Id, staffUser1.Id, Utc(2026,5,20),  "Paid",    null,
            new[] { (part7.Id,2,part7.UnitPrice),  (part9.Id,2,part9.UnitPrice),  (part4.Id,4,part4.UnitPrice) });   // 9,200 → discount
        await EnsureSalesInvoice(db, "INV-20260522001", cust18.Id, staffUser2.Id, Utc(2026,5,22),  "Paid",    null,
            new[] { (part16.Id,1,part16.UnitPrice),(part2.Id,2,part2.UnitPrice),  (part12.Id,2,part12.UnitPrice) }); // 6,640 → discount
        await EnsureSalesInvoice(db, "INV-20260524001", cust19.Id, staffUser3.Id, Utc(2026,5,24),  "Credit",  Utc(2026,6,24),
            new[] { (part5.Id,3,part5.UnitPrice),  (part17.Id,1,part17.UnitPrice),(part25.Id,2,part25.UnitPrice) });
        await EnsureSalesInvoice(db, "INV-20260526001", cust14.Id, staffUser1.Id, Utc(2026,5,26),  "Paid",    null,
            new[] { (part10.Id,1,part10.UnitPrice),(part7.Id,1,part7.UnitPrice) });                                   // 5,700 → discount
        // Today — daily report (3 invoices)
        await EnsureSalesInvoice(db, "INV-20260530001", cust1.Id,  staffUser1.Id, DateTime.UtcNow, "Paid",    null,
            new[] { (part1.Id,2,part1.UnitPrice),  (part8.Id,3,part8.UnitPrice),  (part4.Id,4,part4.UnitPrice) });   // 4,540
        await EnsureSalesInvoice(db, "INV-20260530002", cust6.Id,  staffUser1.Id, DateTime.UtcNow, "Paid",    null,
            new[] { (part7.Id,2,part7.UnitPrice),  (part9.Id,1,part9.UnitPrice) });                                   // 6,200 → discount
        await EnsureSalesInvoice(db, "INV-20260530003", cust16.Id, staffUser4.Id, DateTime.UtcNow, "Paid",    null,
            new[] { (part7.Id,1,part7.UnitPrice),  (part16.Id,1,part16.UnitPrice),(part9.Id,1,part9.UnitPrice) });   // 7,800 → discount

        // ── 9. Appointments (20+ total) ───────────────────────────────────────────
        if (!await db.Appointments.AnyAsync())
        {
            var veh1 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 1 CHA 1234");
            var veh2 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 2 CHA 5678");
            var veh3 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 3 PA 9012");
            var veh4 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 4 CHA 3456");
            var veh5 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 5 JA 7890");
            var veh6 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "GA 1 CHA 1111");
            db.Appointments.AddRange(
                new Appointment { CustomerId = cust1.Id, VehicleId = veh1.Id, AppointmentDate = DateTime.UtcNow.AddDays(-30), ServiceType = "Oil Change",       Description = "Full synthetic oil change + filter",    Status = "Completed" },
                new Appointment { CustomerId = cust1.Id, VehicleId = veh2.Id, AppointmentDate = DateTime.UtcNow.AddDays(-15), ServiceType = "Brake Inspection", Description = "Front brake pad check and replacement",  Status = "Completed" },
                new Appointment { CustomerId = cust2.Id, VehicleId = veh3.Id, AppointmentDate = DateTime.UtcNow.AddDays(-7),  ServiceType = "Tune-Up",          Description = "Spark plug, air filter, throttle clean", Status = "Completed" },
                new Appointment { CustomerId = cust3.Id, VehicleId = veh4.Id, AppointmentDate = DateTime.UtcNow.AddDays(2),   ServiceType = "Timing Belt",      Description = "Timing belt replacement at 60k km",      Status = "Pending"   },
                new Appointment { CustomerId = cust4.Id, VehicleId = veh5.Id, AppointmentDate = DateTime.UtcNow.AddDays(5),   ServiceType = "General Service",  Description = "Full service check",                     Status = "Pending"   },
                new Appointment { CustomerId = cust5.Id, VehicleId = veh6.Id, AppointmentDate = DateTime.UtcNow.AddDays(-3),  ServiceType = "Battery Check",    Description = "Battery voltage test and replacement",   Status = "Completed" }
            );
            await db.SaveChangesAsync();
        }

        // Phase-2 appointments (idempotent: check customer+serviceType+date)
        async Task AddAppt(Guid custId, string vNum, string sType, string desc, string status, DateTime date)
        {
            var veh = await db.CustomerVehicles.FirstOrDefaultAsync(v => v.VehicleNumber == vNum);
            if (veh == null) return;
            if (!await db.Appointments.AnyAsync(a => a.CustomerId == custId && a.ServiceType == sType && a.AppointmentDate == date))
            {
                db.Appointments.Add(new Appointment { CustomerId = custId, VehicleId = veh.Id, AppointmentDate = date, ServiceType = sType, Description = desc, Status = status });
                await db.SaveChangesAsync();
            }
        }

        await AddAppt(cust6.Id,  "BA 7 CHA 2222",  "Oil Change",          "Engine oil and filter change for Thar",              "Completed", DateTime.UtcNow.AddDays(-20));
        await AddAppt(cust7.Id,  "BA 8 JA 3333",   "Chain Replacement",   "Drive chain and sprocket replacement for Duke",      "Completed", DateTime.UtcNow.AddDays(-18));
        await AddAppt(cust8.Id,  "GA 2 CHA 4444",  "General Service",     "50k full service for Activa",                        "Completed", DateTime.UtcNow.AddDays(-12));
        await AddAppt(cust9.Id,  "GA 3 PA 5555",   "Brake Service",       "Front and rear brake pad replacement",               "Completed", DateTime.UtcNow.AddDays(-10));
        await AddAppt(cust10.Id, "BA 9 CHA 6666",  "Suspension Check",    "Inspect and replace front suspension bushings",      "Completed", DateTime.UtcNow.AddDays(-8));
        await AddAppt(cust11.Id, "BA 10 PA 1010",  "Full Service",        "Annual service for Honda City",                      "Completed", DateTime.UtcNow.AddDays(-25));
        await AddAppt(cust12.Id, "BA 11 CHA 2020", "Oil Change",          "10W-40 oil change for Yamaha R15 V4",                "Completed", DateTime.UtcNow.AddDays(-22));
        await AddAppt(cust13.Id, "BA 12 JA 3030",  "Timing Belt",         "Timing belt replacement at 80k km for Fortuner",    "Completed", DateTime.UtcNow.AddDays(-14));
        await AddAppt(cust14.Id, "GA 4 CHA 4040",  "Brake Inspection",    "Full brake and disc rotor inspection for Creta",     "Pending",   DateTime.UtcNow.AddDays(3));
        await AddAppt(cust15.Id, "GA 5 PA 5050",   "Chain Replacement",   "Chain and sprocket kit for Avenger 220",             "Pending",   DateTime.UtcNow.AddDays(1));
        await AddAppt(cust16.Id, "BA 13 CHA 6060", "AC Service",          "Air conditioning check and recharge for Ertiga",    "Pending",   DateTime.UtcNow.AddDays(4));
        await AddAppt(cust17.Id, "BA 14 JA 7070",  "General Service",     "Regular 6-month service for Honda Dio",              "Completed", DateTime.UtcNow.AddDays(-5));
        await AddAppt(cust18.Id, "BA 15 CHA 8080", "Engine Diagnostics",  "OBD-II diagnostic scan for check engine light",     "Pending",   DateTime.UtcNow.AddDays(7));
        await AddAppt(cust19.Id, "GA 6 CHA 9090",  "Suspension Service",  "Fork oil change and rear shock inspection",         "Completed", DateTime.UtcNow.AddDays(-2));
        await AddAppt(cust20.Id, "BA 16 PA 1001",  "Oil Change",          "Fully synthetic oil change for Tata Harrier",       "Pending",   DateTime.UtcNow.AddDays(6));

        // ── 10. Reviews (15+ total) ───────────────────────────────────────────────
        if (!await db.Reviews.AnyAsync())
        {
            var appts = await db.Appointments.Where(a => a.Status == "Completed").ToListAsync();
            var reviews = new List<Review>();
            if (appts.Count > 0) reviews.Add(new Review { CustomerId = cust1.Id, AppointmentId = appts[0].Id, Rating = 5, Comment = "Excellent service, very professional staff!",    Date = DateTime.UtcNow.AddDays(-28) });
            if (appts.Count > 1) reviews.Add(new Review { CustomerId = cust1.Id, AppointmentId = appts[1].Id, Rating = 4, Comment = "Good work on the brakes, quick turnaround.",      Date = DateTime.UtcNow.AddDays(-13) });
            if (appts.Count > 2) reviews.Add(new Review { CustomerId = cust2.Id, AppointmentId = appts[2].Id, Rating = 5, Comment = "Bike runs perfectly after the tune-up.",           Date = DateTime.UtcNow.AddDays(-5)  });
            if (appts.Count > 3) reviews.Add(new Review { CustomerId = cust5.Id, AppointmentId = appts[3].Id, Rating = 3, Comment = "Battery replaced but took longer than expected.",  Date = DateTime.UtcNow.AddDays(-1)  });
            if (reviews.Count > 0) { db.Reviews.AddRange(reviews); await db.SaveChangesAsync(); }
        }

        async Task AddReview(Guid custId, string sType, int rating, string comment, DateTime date)
        {
            var appt = await db.Appointments.FirstOrDefaultAsync(a => a.CustomerId == custId && a.ServiceType == sType && a.Status == "Completed");
            if (appt == null) return;
            if (!await db.Reviews.AnyAsync(r => r.CustomerId == custId && r.AppointmentId == appt.Id))
            {
                db.Reviews.Add(new Review { CustomerId = custId, AppointmentId = appt.Id, Rating = rating, Comment = comment, Date = date });
                await db.SaveChangesAsync();
            }
        }

        await AddReview(cust6.Id,  "Oil Change",         5, "Great service! The Thar runs very smoothly now.",           DateTime.UtcNow.AddDays(-19));
        await AddReview(cust7.Id,  "Chain Replacement",  4, "Precise work on the chain, very satisfied.",                DateTime.UtcNow.AddDays(-17));
        await AddReview(cust8.Id,  "General Service",    5, "Thorough service, staff very helpful and knowledgeable.",   DateTime.UtcNow.AddDays(-11));
        await AddReview(cust9.Id,  "Brake Service",      4, "Brakes feel much better, good job!",                        DateTime.UtcNow.AddDays(-9));
        await AddReview(cust10.Id, "Suspension Check",   3, "Service was okay, waited a bit longer than expected.",      DateTime.UtcNow.AddDays(-7));
        await AddReview(cust11.Id, "Full Service",       5, "Best service center in Kathmandu, highly recommended!",     DateTime.UtcNow.AddDays(-24));
        await AddReview(cust12.Id, "Oil Change",         4, "Quick and efficient oil change, no complaints.",            DateTime.UtcNow.AddDays(-21));
        await AddReview(cust13.Id, "Timing Belt",        5, "Very professional job, Fortuner runs like new!",            DateTime.UtcNow.AddDays(-13));
        await AddReview(cust17.Id, "General Service",    4, "Good overall service for the scooter.",                     DateTime.UtcNow.AddDays(-4));
        await AddReview(cust19.Id, "Suspension Service", 5, "Fork oil change made a huge difference in ride quality!",  DateTime.UtcNow.AddDays(-1));

        // ── 11. Unavailable Part Requests (10+ total) ─────────────────────────────
        if (!await db.UnavailablePartRequests.AnyAsync())
        {
            var v2 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 2 CHA 5678");
            var v4 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 4 CHA 3456");
            var v5 = await db.CustomerVehicles.FirstAsync(v => v.VehicleNumber == "BA 5 JA 7890");
            db.UnavailablePartRequests.AddRange(
                new UnavailablePartRequest { CustomerId = cust1.Id, VehicleId = v2.Id, PartName = "Catalytic Converter", Description = "OEM cat converter for Corolla 2018",  Urgency = "High",   Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-5) },
                new UnavailablePartRequest { CustomerId = cust3.Id, VehicleId = v4.Id, PartName = "ABS Sensor (Rear)",   Description = "Rear ABS sensor for i20 2020",        Urgency = "Medium", Status = "InProgress", RequestDate = DateTime.UtcNow.AddDays(-3) },
                new UnavailablePartRequest { CustomerId = cust4.Id, VehicleId = v5.Id, PartName = "Carburetor Jet Kit",  Description = "Performance jet kit for Pulsar 150",  Urgency = "Low",    Status = "Pending",    RequestDate = DateTime.UtcNow.AddDays(-1) }
            );
            await db.SaveChangesAsync();
        }

        async Task AddPartRequest(Guid custId, string vNum, string partName, string desc, string urgency, string status, DateTime reqDate)
        {
            var veh = await db.CustomerVehicles.FirstOrDefaultAsync(v => v.VehicleNumber == vNum);
            if (veh == null) return;
            if (!await db.UnavailablePartRequests.AnyAsync(r => r.CustomerId == custId && r.PartName == partName))
            {
                db.UnavailablePartRequests.Add(new UnavailablePartRequest { CustomerId = custId, VehicleId = veh.Id, PartName = partName, Description = desc, Urgency = urgency, Status = status, RequestDate = reqDate });
                await db.SaveChangesAsync();
            }
        }

        await AddPartRequest(cust13.Id, "BA 12 JA 3030",  "Intercooler Hose Kit",  "Upper and lower intercooler hoses for Fortuner 2019",   "High",   "Pending",    DateTime.UtcNow.AddDays(-4));
        await AddPartRequest(cust14.Id, "GA 4 CHA 4040",  "EGR Valve",             "EGR valve replacement for Creta 1.6 diesel",            "High",   "InProgress", DateTime.UtcNow.AddDays(-6));
        await AddPartRequest(cust16.Id, "BA 13 CHA 6060", "AC Compressor",         "AC compressor for Ertiga 2021",                         "Medium", "Pending",    DateTime.UtcNow.AddDays(-2));
        await AddPartRequest(cust18.Id, "BA 15 CHA 8080", "Transfer Case Motor",   "4WD transfer case actuator motor for Scorpio",          "Low",    "Pending",    DateTime.UtcNow.AddDays(-1));
        await AddPartRequest(cust19.Id, "GA 6 CHA 9090",  "Quickshifter Module",   "Plug-and-play quickshifter for KTM 390",                "Medium", "InProgress", DateTime.UtcNow.AddDays(-3));
        await AddPartRequest(cust20.Id, "BA 16 PA 1001",  "Sunroof Motor",         "Electric sunroof motor assembly for Harrier 2022",      "Low",    "Pending",    DateTime.UtcNow.AddDays(-5));
        await AddPartRequest(cust2.Id,  "BA 17 CHA 2002", "Tow Bar Assembly",      "Aftermarket tow bar for Hilux 2020",                    "Low",    "Pending",    DateTime.UtcNow.AddDays(-7));

        // ── 12. Service History (16+ total) ───────────────────────────────────────
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

        async Task AddServiceHistory(Guid custId, string vNum, string sType, string desc, string tech, decimal cost, DateTime sDate)
        {
            var veh = await db.CustomerVehicles.FirstOrDefaultAsync(v => v.VehicleNumber == vNum);
            if (veh == null) return;
            if (!await db.ServiceHistories.AnyAsync(h => h.CustomerId == custId && h.ServiceType == sType && h.ServiceDate == sDate))
            {
                db.ServiceHistories.Add(new ServiceHistory { CustomerId = custId, VehicleId = veh.Id, ServiceType = sType, Description = desc, Technician = tech, Cost = cost, ServiceDate = sDate, Status = "Completed" });
                await db.SaveChangesAsync();
            }
        }

        await AddServiceHistory(cust6.Id,  "BA 7 CHA 2222",  "Oil Change",         "Engine oil and filter for Mahindra Thar",              "David Rai",     2800m, DateTime.UtcNow.AddDays(-20));
        await AddServiceHistory(cust7.Id,  "BA 8 JA 3333",   "Chain Replacement",  "Drive chain and sprocket set installed",               "Frank Lama",    2600m, DateTime.UtcNow.AddDays(-18));
        await AddServiceHistory(cust8.Id,  "GA 2 CHA 4444",  "General Service",    "Full 50k service: oil, filter, spark plugs",           "Alice Johnson", 3200m, DateTime.UtcNow.AddDays(-12));
        await AddServiceHistory(cust9.Id,  "GA 3 PA 5555",   "Brake Service",      "Front and rear brake pad replacement + fluid flush",   "Bob Smith",     2100m, DateTime.UtcNow.AddDays(-10));
        await AddServiceHistory(cust10.Id, "BA 9 CHA 6666",  "Suspension Check",   "Front suspension bush kit replaced, alignment done",   "David Rai",     4200m, DateTime.UtcNow.AddDays(-8));
        await AddServiceHistory(cust11.Id, "BA 10 PA 1010",  "Full Service",       "Annual service: oil, filters, belts, coolant check",   "Carol Thapa",   3800m, DateTime.UtcNow.AddDays(-25));
        await AddServiceHistory(cust12.Id, "BA 11 CHA 2020", "Oil Change",         "10W-40 semi-synthetic oil change + air filter",        "Frank Lama",    1400m, DateTime.UtcNow.AddDays(-22));
        await AddServiceHistory(cust13.Id, "BA 12 JA 3030",  "Timing Belt",        "Timing belt + water pump replaced at 80k km",         "Bob Smith",     6500m, DateTime.UtcNow.AddDays(-14));
        await AddServiceHistory(cust17.Id, "BA 14 JA 7070",  "General Service",    "6-month service: oil, brake check, chain lube",        "Alice Johnson", 1200m, DateTime.UtcNow.AddDays(-5));
        await AddServiceHistory(cust19.Id, "GA 6 CHA 9090",  "Suspension Service", "Fork oil changed, rear shock damping adjusted",       "David Rai",     3100m, DateTime.UtcNow.AddDays(-2));
        await AddServiceHistory(cust1.Id,  "BA 2 CHA 5678",  "AC Service",         "AC gas recharge + condenser cleaned for Corolla",     "Emma Shrestha", 2200m, DateTime.UtcNow.AddDays(-45));
        await AddServiceHistory(cust3.Id,  "BA 4 CHA 3456",  "Full Service",       "Annual service: all fluids, filters, spark plugs",    "Carol Thapa",   4500m, DateTime.UtcNow.AddDays(-60));

        // ── 13. Notifications ─────────────────────────────────────────────────────
        if (!await db.Notifications.AnyAsync())
        {
            db.Notifications.AddRange(
                new Notification { Title = "New Part Request", Message = "Customer Ravi Kumar requested Catalytic Converter.", Type = "Request", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Notification { Title = "New Review",       Message = "Ravi Kumar left a 5-star review.",                  Type = "Review",  IsRead = true,  CreatedAt = DateTime.UtcNow.AddDays(-28) }
            );
            await db.SaveChangesAsync();
        }

        // Low-stock notifications (idempotent per title+type)
        foreach (var (title, message, hoursAgo) in new (string, string, int)[]
        {
            ("Low Stock: Throttle Body Gasket",            $"Throttle Body Gasket has only 3 units (reorder: 5). Please reorder soon.",               1),
            ("Low Stock: Throttle Body Gasket (Critical)", $"CRITICAL: Throttle Body Gasket ({part13.PartCode}) at 3 units — below reorder threshold.", 2),
            ("Low Stock: Suspension Bush Kit",             $"Suspension Bush Kit has only 7 units in stock (reorder: 10). Consider restocking.",        3),
            ("Low Stock: Suspension Bush Kit (Alert)",     $"Suspension Bush Kit ({part14.PartCode}) stock (7 units) is below reorder level of 10.",    4),
            ("Low Stock: Coolant Temperature Sensor",      $"Coolant Temperature Sensor has 9 units remaining (reorder: 12). Reorder recommended.",    5),
            ("Low Stock: Ignition Coil",                   $"CRITICAL: Ignition Coil ({part21.PartCode}) at 2 units — below reorder threshold of 5.",  6),
            ("Low Stock: Exhaust Gasket Set",              $"Exhaust Gasket Set ({part23.PartCode}) has 4 units (reorder: 6). Please restock.",         7),
        })
        {
            if (!await db.Notifications.AnyAsync(n => n.Title == title && n.Type == "LowStock"))
                db.Notifications.Add(new Notification { Title = title, Message = message, Type = "LowStock", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-hoursAgo) });
        }

        foreach (var (title, message, type, isRead, daysAgo) in new (string, string, string, bool, int)[]
        {
            ("New Appointment",      "Hari Thapa booked a Timing Belt service for 2 days from now.",     "Appointment", false, 0),
            ("Credit Reminder Sent", "Overdue credit reminder sent to Bijaya Magar (INV-20260415001).",   "CreditAlert", true,  1),
            ("Credit Reminder Sent", "Overdue credit reminder sent to Suresh Tamang (INV-20260410001).",  "CreditAlert", true,  1),
            ("New Review",           "Bikash Gurung left a 5-star review for Battery Check service.",     "Review",      true,  3),
            ("Stock Replenished",    "Timing Belt restocked to 15 units after latest purchase receipt.",  "StockUpdate", true,  10),
            ("Invoice Emailed",      "Sales invoice INV-20260530002 emailed to Ram Bahadur Kshetri.",     "Email",       true,  0),
        })
        {
            if (!await db.Notifications.AnyAsync(n => n.Title == title && n.Message == message))
                db.Notifications.Add(new Notification { Title = title, Message = message, Type = type, IsRead = isRead, CreatedAt = DateTime.UtcNow.AddDays(-daysAgo) });
        }
        await db.SaveChangesAsync();

        // ── 14. Email Logs ────────────────────────────────────────────────────────
        if (!await db.EmailLogs.AnyAsync())
        {
            db.EmailLogs.AddRange(
                new EmailLog { ToEmail = "customer3@vparts.com",   Subject = "Overdue Credit Reminder - INV-20260320001", Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-1)  },
                new EmailLog { ToEmail = "krixh.dhakal@gmail.com", Subject = "Overdue Credit Reminder - INV-20260415001", Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-1)  },
                new EmailLog { ToEmail = "krixh.dhakal@gmail.com", Subject = "Overdue Credit Reminder - INV-20260410001", Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-1)  },
                new EmailLog { ToEmail = "customer4@vparts.com",   Subject = "Overdue Credit Reminder - INV-20260401001", Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-2)  },
                new EmailLog { ToEmail = "customer6@vparts.com",   Subject = "Your Invoice INV-20260530002 - VParts",     Status = "Sent",   SentAt = DateTime.UtcNow               },
                new EmailLog { ToEmail = "customer1@vparts.com",   Subject = "Your Invoice INV-20260530001 - VParts",     Status = "Sent",   SentAt = DateTime.UtcNow               },
                new EmailLog { ToEmail = "customer5@vparts.com",   Subject = "Your Invoice INV-20260519001 - VParts",     Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-11) },
                new EmailLog { ToEmail = "customer17@vparts.com",  Subject = "Your Invoice INV-20260520001 - VParts",     Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-10) },
                new EmailLog { ToEmail = "customer18@vparts.com",  Subject = "Your Invoice INV-20260522001 - VParts",     Status = "Sent",   SentAt = DateTime.UtcNow.AddDays(-8)  },
                new EmailLog { ToEmail = "invalid@@broken.c",      Subject = "Invoice Delivery Test",                     Status = "Failed", SentAt = DateTime.UtcNow.AddDays(-3)  }
            );
            await db.SaveChangesAsync();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

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

    private static async Task<ApplicationUser> EnsureCustomerUser(
        UserManager<ApplicationUser> um, string email, string fullName, string phone)
    {
        var user = await um.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, PhoneNumber = phone };
            await um.CreateAsync(user, "Cust@123");
            await um.AddToRoleAsync(user, "Customer");
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
        db.CustomerVehicles.Add(new CustomerVehicle { CustomerId = customerId, VehicleNumber = vehicleNumber, Brand = brand, Model = model, VehicleType = vehicleType });
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

    private static async Task EnsureSalesInvoice(
        ApplicationDbContext db, string invoiceNumber, Guid customerId, Guid staffId,
        DateTime date, string paymentStatus, DateTime? creditDueDate,
        (Guid PartId, int Qty, decimal Price)[] items)
    {
        if (await db.SalesInvoices.AnyAsync(s => s.InvoiceNumber == invoiceNumber)) return;
        var si = new SalesInvoice {
            InvoiceNumber = invoiceNumber, CustomerId = customerId, StaffId = staffId,
            Date = date, PaymentStatus = paymentStatus, CreditDueDate = creditDueDate,
            Items = items.Select(i => new SalesInvoiceItem { PartId = i.PartId, Quantity = i.Qty, UnitPrice = i.Price }).ToList()
        };
        si.Subtotal    = si.Items.Sum(i => i.Quantity * i.UnitPrice);
        si.Discount    = si.Subtotal > 5000 ? si.Subtotal * 0.10m : 0;
        si.TotalAmount = si.Subtotal - si.Discount;
        db.SalesInvoices.Add(si);
        await db.SaveChangesAsync();
    }
}
