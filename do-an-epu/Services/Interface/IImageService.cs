namespace do_an_epu.Services.Interface
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string? name = null);
        Task<List<string>> UploadMultipleAsync(IEnumerable<IFormFile> files,string folder);
        Task DeleteAsync(string publicId);
    }
}
