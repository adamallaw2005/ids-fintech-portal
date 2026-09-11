using MyWebsite_API.Services;

var service = new PasswordHashService();
var hash = service.HashPassword("review-test-password");
if (!service.VerifyPassword("review-test-password", hash))
    throw new Exception("Correct password was rejected.");
if (service.VerifyPassword("wrong-password", hash))
    throw new Exception("Wrong password was accepted.");
if (hash == service.HashPassword("review-test-password"))
    throw new Exception("Passwords must receive independent salts.");

string[] invalidHashes = [
    "", "CHANGE_THIS_HASH_IN_BACKEND", "PBKDF2-SHA256$abc$a$b",
    "PBKDF2-SHA256$0$a$b", "PBKDF2-SHA256$-1$a$b",
    "PBKDF2-SHA256$120000$invalid$invalid", "PBKDF2-SHA256$120000$$"
];
foreach (var invalid in invalidHashes)
    if (service.VerifyPassword("review-test-password", invalid))
        throw new Exception("Malformed hash was accepted.");

Console.WriteLine("Passed 10 password hashing checks.");
