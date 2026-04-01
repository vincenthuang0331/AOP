# InfraStack.Utility.AOP

基於 PostSharp 的 AOP 工具庫，提供方法攔截、快取管理與依賴注入功能。

## 功能特色

- **方法攔截** - 透過 `MethodBoundaryAttribute` 實現 AOP 攔截
- **智慧快取** - 支援多種快取策略與多儲存庫管理
- **非同步支援** - 完整支援 `Task`、`Task<T>` 與同步方法
- **執行緒安全** - 內建 SemaphoreSlim 防止快取擊穿

## 快速開始

### 安裝

**從 GitHub Packages 安裝：**

1. 建立或編輯 `nuget.config`：
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/vincenthuang0331/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

2. 安裝套件：
```bash
dotnet add package InfraStack.Utility.AOP
```

> **注意：** 需要有 GitHub Personal Access Token (PAT) 並勾選 `read:packages` 權限

### 註冊快取儲存庫

```csharp
services.ConfigureCacheRepository<MemoryCacheRepository>(0, duration: 300)
        .ConfigureCacheRepository<RedisCacheRepository>(1, duration: 3600);
```

### 使用快取 Attribute

```csharp
public class UserService
{
    // 自動產生快取 Key（含參數值）
    [Cache(QueryParamAsKey = true)]
    public async Task<User> GetUserAsync(int userId)
    {
        return await _repository.FindAsync(userId);
    }

    // 手動指定 Key 模板
    [Cache(Key = "Product_{productId}")]
    public Product GetProduct(int productId)
    {
        return _repository.GetProduct(productId);
    }

    // 使用第二組儲存庫（Redis），快取 1 小時
    [Cache(CacheRepositoryEnum = 1, Duration = 3600)]
    public List<Category> GetCategories()
    {
        return _repository.GetAllCategories();
    }
}
```

### 快取策略

透過方法參數控制快取行為：

```csharp
[Cache]
public List<Item> GetItems(CacheEnum cacheEnum = CacheEnum.Normal)
{
    return _repository.GetItems();
}

// 強制更新快取
var items = GetItems(CacheEnum.ForceUpdate);

// 清除快取
GetItems(CacheEnum.Expire);

// 只從快取取（無則回傳 null，不執行方法）
var cached = GetItems(CacheEnum.AlwaysFromCache);
```

## 實作自訂快取儲存庫

```csharp
public class MemoryCacheRepository : ICacheRepository
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public Task AddAsync<T>(string Key, T Value, int Duration)
    {
        var options = Duration switch
        {
            -1 => new MemoryCacheEntryOptions(),
            -2 => new MemoryCacheEntryOptions { 
                AbsoluteExpiration = DateTime.Today.AddDays(1) 
            },
            _ => new MemoryCacheEntryOptions { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Duration) 
            }
        };
        _cache.Set(Key, Value, options);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string Key)
    {
        _cache.TryGetValue(Key, out T? value);
        return Task.FromResult(value);
    }

    public Task<bool> DeleteAsync(string Key)
    {
        _cache.Remove(Key);
        return Task.FromResult(true);
    }
}
```

## 進階功能

### 關閉執行緒鎖定

```csharp
[Cache(IsLock = false)]
public async Task<Data> GetDataAsync()
{
    // 允許多執行緒同時執行（不等待其他 thread）
}
```

### 監控 Lock 狀態

```csharp
var waitingThreads = CacheAttribute.GetWaitCount();
foreach (var (CacheKey, WaitCount) in waitingThreads)
{
    Console.WriteLine($"Key: {CacheKey}, Waiting: {WaitCount}");
}
```

## 發布新版本

專案使用 GitHub Actions 自動打包並發布到 GitHub Packages。

### 發布流程

1. 更新 `InfraStack.Utility.AOP.csproj` 的版本號
2. Commit 變更
3. 建立 tag 並 push：
```bash
git tag v1.0.1
git push origin v1.0.1
```

GitHub Actions 會自動：
- 建置專案
- 執行測試
- 打包 NuGet
- 發布到 GitHub Packages

### 手動觸發

也可以在 GitHub Actions 頁面手動執行 "Publish NuGet Package" workflow。

## 授權

MIT License - Copyright © 2026 vincenthuang0331

## 相依套件

- PostSharp 2025.0.6
- Microsoft.Extensions.DependencyInjection 9.0.3
