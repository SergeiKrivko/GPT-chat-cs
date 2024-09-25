using Utils;
using Utils.Http;

namespace Core.Releases;

public class ReleasesService : HttpService
{
    private static ReleasesService? _instance;

    public static ReleasesService Instance
    {
        get
        {
            _instance ??= new ReleasesService();
            return _instance;
        }
    }

    private ReleasesService()
    {
        BaseUrl = Config.BaseUrl;
    }

    public async Task<List<ReleaseReadModel>> GetAll()
    {
        return await Get<List<ReleaseReadModel>>("api/v1/releases");
    }

    public async Task<ReleaseAssetReadModel> GetLatest()
    {
        return await Get<ReleaseAssetReadModel>("api/v1/releases/latest?system=");
    }
}