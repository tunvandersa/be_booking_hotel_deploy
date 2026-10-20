using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using do_an_epu.Services.Interface;
namespace do_an_epu.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }
        public async Task<string> UploadAsync (IFormFile file, string? folder = null)
        {
        
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Upload image failed");

            return result.PublicId;
        }
        public async Task<List<string>> UploadMultipleAsync(
        IEnumerable<IFormFile> files,
        string folder)
        {
            var imageUrls = new List<string>();

            foreach (var file in files)
            {
                var url = await UploadAsync(file, folder);
                imageUrls.Add(url);
            }

            return imageUrls;
        }
        public async Task DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("PublicId is required");

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result != "ok")
                throw new Exception("Delete image failed");
        }
    }
}
