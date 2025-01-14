using Microsoft.EntityFrameworkCore; // 提供與資料庫交互的功能 | Provides functionality to interact with the database
using Microsoft.Extensions.Configuration; // 用來讀取 appsettings.json 配置 | Used to read appsettings.json configuration
using Microsoft.Extensions.DependencyInjection; // DI 容器，用於注入服務 | Dependency Injection container for service registration
using Microsoft.Extensions.Logging; // 日誌系統 | Logging system
using System; // 系統基本功能 | Basic system functionalities
using System.Collections.Generic; // 用於處理清單 | For handling lists
using System.ComponentModel.DataAnnotations;
using System.IO; // 處理檔案系統 | For file system handling
using System.Linq; // 提供集合操作方法 | Provides LINQ methods for collections
using System.Threading.Tasks; // 支援非同步操作 | Supports asynchronous operations

namespace QRCodeUrlGenerator
{
    internal class Program
    {
        /// <summary>
        /// 程式主入口 | Main entry point of the program
        /// </summary>
        static async Task Main(string[] args)
        {
            // 建立 DI 容器，設定日誌和資料庫服務 | Create DI container to configure logging and database services
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole()) // 日誌輸出到控制台 | Log output to console
                .AddDbContext<AppDbContext>((serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")); // 通用連線字串名稱 | General connection string
                })
                .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // 設定路徑為專案路徑 | Set path to project directory
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 讀取 appsettings.json | Read appsettings.json
                    .Build())
                .BuildServiceProvider(); // 建立 DI 容器 | Build DI container

            var context = serviceProvider.GetRequiredService<AppDbContext>(); // 從 DI 容器中取得資料庫上下文 | Get database context from DI container
            var urlGeneratorService = new UrlGeneratorService(context); // 建立 UrlGeneratorService 實例 | Create UrlGeneratorService instance

            // 迴圈允許用戶多次選擇操作 | Loop allows users to select operations multiple times
            while (true)
            {
                Console.WriteLine("請選擇操作模式: | Select operation mode:");
                Console.WriteLine("1. 產生並檢視少量亂碼 | Generate and preview random URLs");
                Console.WriteLine("2. 插入資料庫 | Insert into database");
                Console.WriteLine("3. 離開 | Exit");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Write("請輸入要產生的亂碼數量: | Enter the number of random URLs to generate: ");
                        if (int.TryParse(Console.ReadLine(), out int previewCount))
                        {
                            Console.WriteLine("產生的亂碼如下: | Generated random URLs:");
                            var previewUrls = await urlGeneratorService.GenerateAndPreviewRandomUrls(previewCount);
                            foreach (var url in previewUrls)
                            {
                                Console.WriteLine(url);
                            }
                        }
                        else
                        {
                            Console.WriteLine("輸入無效，請輸入有效的數字。| Invalid input. Please enter a valid number.");
                        }
                        break;
                    case "2":
                        Console.Write("請輸入要新增的資料筆數: | Enter the number of records to insert: ");
                        if (int.TryParse(Console.ReadLine(), out int totalInsertCount))
                        {
                            Console.WriteLine($"開始插入 {totalInsertCount} 筆資料... | Inserting {totalInsertCount} records...");
                            await urlGeneratorService.InsertUrls(totalInsertCount); // 插入資料 | Insert data
                            Console.WriteLine("資料插入完成。| Data insertion completed.");
                        }
                        else
                        {
                            Console.WriteLine("輸入無效，請輸入有效的數字。| Invalid input. Please enter a valid number.");
                        }
                        break;
                    case "3":
                        Console.WriteLine("程式結束。| Program terminated.");
                        return; // 離開程式 | Exit program
                    default:
                        Console.WriteLine("無效的選項，請重新選擇。| Invalid option. Please select again.");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 資料庫上下文類別，負責與資料庫互動 | Database context class for interacting with the database
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// QR Code 網址記錄表 | QR Code URL records table
        /// </summary>
        public DbSet<UrlRecord> UrlRecords { get; set; }

        /// <summary>
        /// 配置資料表結構 | Configures the table structure
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UrlRecord>(entity =>
            {
                entity.ToTable("QRCodeTable"); // 資料表名稱 | Table name
                entity.HasKey(e => e.Id).HasName("PK_QRCodeTable"); // 主鍵設定 | Primary key setup
                entity.Property(e => e.FullUrl).HasColumnName("FullUrl"); // 網址欄位 | URL column
                entity.Property(e => e.Code).HasColumnName("Code"); // 隨機碼欄位 | Random code column
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt"); // 建立時間欄位 | Created time column
            });
        }
    }

    /// <summary>
    /// QR Code 網址記錄實體類別 | Entity class for QR Code URL records
    /// </summary>
    public class UrlRecord
    {
        [Key] // 主鍵 | Primary key
        public int Id { get; set; }

        /// <summary>
        /// 完整的 QR Code 網址 | Full QR Code URL
        /// </summary>
        public string FullUrl { get; set; }

        /// <summary>
        /// 隨機碼 | Random code
        /// </summary>
        [Required]
        [MaxLength(16)] // 最大長度為 16 | Maximum length is 16
        public string Code { get; set; }

        /// <summary>
        /// 建立時間 | Time of creation
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 隨機字串生成工具類別 | Utility class for generating random strings
    /// </summary>
    public static class RandomStringGenerator
    {
        private static Random _random = new Random(); // 隨機數生成器 | Random number generator

        /// <summary>
        /// 生成指定長度的隨機字串，包含大寫字母和數字 | Generates a random string of specified length with uppercase letters and digits
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // 可用字符集 | Available character set
            char[] stringChars = new char[length];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }
            return new string(stringChars); // 返回生成的字串 | Return the generated string
        }
    }

    /// <summary>
    /// 處理 QR Code 網址生成和插入邏輯的服務 | Service for handling QR Code URL generation and insertion logic
    /// </summary>
    public class UrlGeneratorService
    {
        private readonly AppDbContext _context; // 注入資料庫上下文 | Inject database context

        public UrlGeneratorService(AppDbContext context)
        {
            _context = context; // 設定資料庫上下文 | Set database context
        }

        /// <summary>
        /// 插入隨機網址到資料庫 | Insert random URLs into the database
        /// </summary>
        public async Task InsertUrls(int count)
        {
            // 從資料庫中取得已存在的亂碼 | Retrieve existing codes from the database
            var existingCodes = (await _context.UrlRecords
                .AsNoTracking()
                .Select(record => record.Code)
                .ToListAsync())
                .ToHashSet();

            // 生成不重複的亂碼 | Generate unique random codes
            List<string> randomCodes = new List<string>();
            while (randomCodes.Count < count)
            {
                var newCode = RandomStringGenerator.GenerateRandomString(10);
                if (!existingCodes.Contains(newCode) && !randomCodes.Contains(newCode))
                {
                    randomCodes.Add(newCode);
                }
            }

            // 建立 URL 資料並插入資料庫 | Create URL records and insert into database
            var urlRecords = randomCodes.Select(code => new UrlRecord
            {
                FullUrl = $"https://yourdomain.com/QR/{code}",
                Code = code,
                CreatedAt = DateTime.Now
            }).ToList();

            await _context.UrlRecords.AddRangeAsync(urlRecords);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 生成並檢視隨機網址，不進行資料庫操作 | Generate and preview random URLs without database operation
        /// </summary>
        public async Task<List<string>> GenerateAndPreviewRandomUrls(int count)
        {
            var existingCodes = (await _context.UrlRecords
                .AsNoTracking()
                .Select(record => record.Code)
                .ToListAsync())
                .ToHashSet();

            List<string> randomUrls = new List<string>();
            while (randomUrls.Count < count)
            {
                var newCode = RandomStringGenerator.GenerateRandomString(10);
                if (!existingCodes.Contains(newCode) && !randomUrls.Contains(newCode))
                {
                    randomUrls.Add($"https://yourdomain.com/QR/{newCode}");
                }
            }

            return randomUrls;
        }
    }
}
