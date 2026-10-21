public class Organizer
{
    public required string Username { get; set; }
    public required string Password { get; set; }//Password should not be public with get/set
    public bool HasChangedPassword { get; set; }
}