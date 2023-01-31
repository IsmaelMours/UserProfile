using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Net.Http.Headers;
using UserProfile.Data;
using UserProfile.Model;
using UserProfile.Services;
using UserProfile.UserDto;

namespace UserProfile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbContext _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        private readonly IauthManager _authManager;
        private readonly IWebHostEnvironment _environment;
        private readonly UserDataContext  _context;
        public UserController(UserManager userManager, ILogger<UserController> logger, IMapper mapper, IauthManager authManager,
                    IWebHostEnvironment environment, UserDataContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _authManager = authManager;
            _environment = environment;
            _context = context;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] User UserDto)
        {
            if (UserDto is null)
            {
                throw new ArgumentNullException(nameof(UserDto));
            }

            _logger.LogInformation($"Registration attempt for {UserDto.Email}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = _mapper.Map<User>(UserDto);
                user.UserName = UserDto.Email;
                var result = await _userManager.CreateAsync(user, UserDto.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }
                List<string> list = new List<string>();
                foreach (var role in UserDto.Roles)
                {
                    list.Add(role);
                }

                await _userManager.AddToRolesAsync(User, list);

                return Accepted(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(Register)}");
                return StatusCode(500, "internal Server Error. Please Contact Your Administrator.");
            }
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto userDto)
        {
            _logger.LogInformation($"Login attempt for {UserDto.Email}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!await _authManager.ValidateUser(UserDto))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByEmailAsync(UserDto.Email);
                var roles = await _userManager.GetRolesAsync(user);

                return Accepted(new { jwtToken = await _authManager.CreateToken(), roles = roles, id = user.Id }); //, id = user.Id 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Wrong login{nameof(Login)}");
                return Problem($"try to log again{nameof(Login)}", statusCode: 500);
            }
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return await GetAllUser();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var entry = await _context.Users.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            return entry;
        }

        // PUT: api/users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            //user.ModifiedDate = DateTime.Now;
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var entry = await _context.Users.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            _context.Users.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EntryExists(string email)
        {
            return (_context.Users?.Any(e => e.Email == email)).GetValueOrDefault();
        }


        private async Task<IActionResult> GetFile([FromRoute] string fileName)
        {
            string path = _environment.WebRootPath + @"Images\";


            if (System.IO.File.Exists(path + fileName + ".png"))
            {
                byte[] b = System.IO.File.ReadAllBytes(path + fileName + ".png");
                return File(b, "image/png");
            }
            else if (System.IO.File.Exists(path + fileName + ".jpg"))
            {
                byte[] b = System.IO.File.ReadAllBytes(path + fileName + ".jpg");
                return File(b, "image/jpg");
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost("upload"), DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim();
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("exportToExcel")]
        public ActionResult<IFormFile> ExportToExcel()
        {
            var users = _context.Users.ToList();
            try
            {
                if (users == null)
                    return NotFound();

                var stream = new MemoryStream();

                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.Add("Users");

                    var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
                    customStyle.Style.Font.UnderLine = true;
                    customStyle.Style.Font.Size = 16;
                    customStyle.Style.Font.Bold = true;
                    customStyle.Style.Font.Color.SetColor(Color.Blue);

                    var startRow = 5;
                    var row = startRow;

                    worksheet.Cells["A1"].Value = "User Profile";
                    using (var rw = worksheet.Cells["A1:D1"])
                    {
                        rw.Merge = true;
                        rw.Style.Font.Color.SetColor(Color.Black);
                        rw.Style.Font.Size = 14;
                        rw.Style.Font.Bold = true;
                       
                    }

                    worksheet.Cells["A4"].Value = "First Name";
                    worksheet.Cells["B4"].Value = "Last Name";
                    worksheet.Cells["C4"].Value = "Email";
                    worksheet.Cells["D4"].Value = "Status";
                  

                    row = 5;
                    foreach (var user in users)
                    {
                        worksheet.Cells[row, 1].Value = user.FirstName;
                        worksheet.Cells[row, 2].Value = user.LastName;
                        worksheet.Cells[row, 3].Value = user.Email;
                        worksheet.Cells[row, 4].Value = user.Status;

                        row++;
                    }

                   

                    xlPackage.Save();
                }

                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users list");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost("batchUpload")]
        public async Task<IActionResult> BatchUpload(IFormFile batchUser)
        {
            if (ModelState.IsValid)
            {
                if (batchUser?.Length > 0)
                {
                    var stream = batchUser.OpenReadStream();

                    List<User> users = new List<User>();

                    try
                    {
                        using (var xlPackage = new ExcelPackage(stream))
                        {
                            var worksheet = xlPackage.Workbook.Worksheets.First();
                            var rowCount = worksheet.Dimension.Rows;

                            for (var row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    var firstName = worksheet.Cells[row, 1].Value?.ToString();
                                    var lastName = worksheet.Cells[row, 2].Value?.ToString();
                                    var email = worksheet.Cells[row, 3].Value?.ToString();
                                    var status = worksheet.Cells[row, 4].Value?.ToString();

                                    var user = new User()
                                    {
                                        FirstName = firstName,
                                        LastName = lastName,
                                        Email = email,
                                        Status = status
                                    };

                                    users.Add(user);
                                }
                                catch (Exception ex)
                                {

                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                        return Ok(users);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return NotFound();
        }

        private async Task<List<User>> GetAllUser()
        {
            return await _context.Users.ToListAsync();
        }
    }
}

