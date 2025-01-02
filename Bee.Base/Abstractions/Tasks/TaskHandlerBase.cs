
using Avalonia.Media.Imaging;

using Bee.Base.Models.Tasks;

using LanguageExt;


namespace Bee.Base.Abstractions.Tasks;

/// <summary>
/// 任务处理抽象基类
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="coverHandler"></param>
public abstract class TaskHandlerBase<T>(ICoverHandler coverHandler) : ITaskHandler<T> where T : TaskArgumentBase
{
    public ICoverHandler CoverHandler { get; } = coverHandler;
    private readonly uint _defaultCoverWidth = 80;
    private readonly uint _defaultCoverHeight = 80;

    /// <summary>
    /// [可重写] 从输入文件路径，创建任务列表
    /// </summary>
    /// <param name="inputPaths"></param>
    /// <param name="arguments">任务参数</param>
    /// <param name="inputExtensions"></param>
    /// <returns></returns>
    public virtual async Task<List<TaskItem>> CreateTasksFromInputPathsAsync(List<string> inputPaths,
        IEnumerable<string>? inputExtensions = null,
        T? arguments = null)
    {
        if (inputPaths == null)
        {
            return [];
        }

        var files = GetFiles(inputPaths, inputExtensions);
        var tasks = new List<TaskItem>();
        foreach (var file in files)
        {
            using Stream stream = await CoverHandler.GetCoverAsync(file, _defaultCoverWidth, _defaultCoverHeight);
            tasks.Add(new TaskItem
            {
                // 任务封面图片
                Source = new Bitmap(stream),
                // 任务名
                Name = Path.GetFileName(file),
                // 文件地址
                //FileName = file,
                Input = file
            });
        }
        return tasks;
    }

    public abstract Task<Fin<Unit>> ExecuteAsync(TaskItem taskItem,
        T arguments,
        Action<double> progressCallback,
        CancellationToken cancellationToken = default)
        ;

    /// <summary>
    /// 从输入的文件路径集合中，获取指定扩展名的文件
    /// </summary>
    /// <param name="inputPaths">输入路径数组</param>
    /// <returns>文件地址集合</returns>
    private List<string> GetFiles(IList<string> inputPaths, IEnumerable<string>? inputExtensions = null)
    {
        var files = new List<string>();
        foreach (var path in inputPaths)
        {
            // 如果是路径
            if (Directory.Exists(path))
            {
                // 查找目录及子目录下所有指定 Extensions 扩展名的文件
                files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(file => inputExtensions?.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) == true))
                    ;
            }
            else if (File.Exists(path))
            {
                // 如果是查找的扩展类型
                if (inputExtensions?.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    files.Add(path);
                }
            }
        }
        return files;
    }
}