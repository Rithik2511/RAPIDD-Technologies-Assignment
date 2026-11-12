using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RAPIDDTechnologiesAssignment.Models;
using SkiaSharp;

namespace RapidTechnologiesAssignment_R.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _clientFactory = clientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> FetchTableData(int skip, int take, CancellationToken token)
        {

            List<EmployeeDTO> employees = new List<EmployeeDTO>();
            List<Employee> tableData = await GetEmployeeDataFromAPI(token);
            if (tableData is { Count: > 0 })
            {
                int counter = 0;
                employees = tableData.GroupBy(emp =>
                {
                    return emp.EmployeeName;
                }).Select((empGroup) =>
                {
                    TimeSpan calcTime = TimeSpan.Zero;
                    empGroup.Select(distEmp => distEmp.EndTimeUtc - distEmp.StarTimeUtc)
                    .ToList()
                    .ForEach(span => calcTime = calcTime.Add(span));
                    return new
                    {
                        EmployeeName = empGroup.Key,
                        TotalTimeWorked = calcTime
                    };
                }).OrderByDescending(employee =>
                {
                    return employee.TotalTimeWorked;
                }).Select(employee =>
                {
                    return new EmployeeDTO()
                    {
                        SrNo = ++counter,
                        EmployeeName = employee.EmployeeName,
                        TotalTimeInHours = employee.TotalTimeWorked.TotalHours.ToString("F2"),
                        TotalTimeWorked = $"TotalDays: {employee.TotalTimeWorked.Days}\nTotalHours: {employee.TotalTimeWorked.TotalHours.ToString("F2")}\nTotalMinutes: {employee.TotalTimeWorked.TotalMinutes}\nTotalSeconds: {employee.TotalTimeWorked.TotalSeconds}\n"
                    };
                }).ToList();
            }
            //tableData = tableData.Skip(skip).Take(take).ToList();
            return Content(JsonConvert.SerializeObject(employees));
        }

        public async Task<IActionResult> CreateChart(CancellationToken token)
        {
            List<Employee> tableData = await GetEmployeeDataFromAPI(token);
            const int imageWidth = 1000;
            const int imageHeight = 1000;
            using SKSurface surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            var dynamicSlices = tableData.GroupBy(emp => emp.EmployeeName)
                .Select(singleEmployeeRecords =>
                {
                    TimeSpan calcTime = TimeSpan.Zero;
                    singleEmployeeRecords.Select(distEmp => distEmp.EndTimeUtc - distEmp.StarTimeUtc)
                    .ToList()
                    .ForEach(span => calcTime = calcTime.Add(span));
                    return new
                    {
                        EmployeeName = singleEmployeeRecords.Key,
                        TotalTimeWorked = float.Parse(calcTime.TotalHours.ToString("F2"))
                    };
                }).ToArray();
            #region Took Inspiration From Internet as i have never worked with this framework or have create charts in dotnet 
            int totalSlices = dynamicSlices.Length;
            float totalValue = dynamicSlices.Sum(emp => emp.TotalTimeWorked);

            SKRect rect = new SKRect(150, 150, 800, 800);
            float startAngle = -90f;

            using var slicePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };
            using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            using var font = new SKFont { Size = 14f, Typeface = typeface };

            for (int index = 0; index < totalSlices; index++)
            {
                var slice = dynamicSlices[index];
                float sweepAngle = (slice.TotalTimeWorked / totalValue) * 360f;

                float hue = (360f / totalSlices) * index;

                slicePaint.Color = SKColor.FromHsl(hue, 75f, 55f);
                // 1. Draw Slice
                using var path = new SKPath();
                path.MoveTo(rect.MidX, rect.MidY);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();
                canvas.DrawPath(path, slicePaint);

                // 2. Text Labeling
                float midAngle = startAngle + (sweepAngle / 2f);
                double radians = Math.PI * midAngle / 180.0;

                // Push small labels slightly further out so they don't overlap in the center
                float labelRadius = 200f;

                float textX = rect.MidX + (float)(labelRadius * Math.Cos(radians));
                float textY = rect.MidY + (float)(labelRadius * Math.Sin(radians));

                string labelText = $"{slice.EmployeeName} ({slice.TotalTimeWorked})";

                float textWidth = font.MeasureText(labelText);
                textX -= textWidth / 2f;
                textY += font.Size / 3f;

                canvas.DrawText(labelText, textX, textY, font, textPaint);

                startAngle += sweepAngle;
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] bytes = data.ToArray();
            #endregion
            return File(bytes, "image/png");
        }

        private async Task<List<Employee>> GetEmployeeDataFromAPI(CancellationToken token)
        {
            List<Employee> employess = new List<Employee>();
            string url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            if (!string.IsNullOrWhiteSpace(url))
            {
                HttpClient client = _clientFactory.CreateClient();
                string data = await client.GetStringAsync(url, token);
                if (!string.IsNullOrWhiteSpace(data))
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    };
                    employess = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employee>>(data, settings);
                }
            }
            return employess;
        }
    }
}
