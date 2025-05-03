# Sync2OSS

### Also available in [English Version](./readme_en.md)

## 简介

Sync2OSS 是一个用于将文件同步到阿里云 OSS 的工具，支持通过 GitHub Actions 以及命令行两种方式使用。当前仅支持阿里云 OSS 作为对象存储服务提供商。

## 功能特性

- 支持通过 GitHub Actions 自动同步文件到阿里云 OSS
- 支持命令行参数灵活上传本地文件/文件夹或 GitHub Release 资源
- 支持软链接（Symlink）功能
- 可自定义保留版本数量、目标目录等参数
- 日志详细，便于排查问题

## 版本说明

- **v2**：始终跟随 v2 分支实时更新，适合需要最新特性和修复的用户。
- **Release Tag**：可在 Release 页面查看并选择具体 tag，获得稳定的固定版本。

> 推荐生产环境使用 Release Tag 版本，保持更新可选用 v2。

## 使用方法

### 1. 作为 GitHub Action 使用

**示例 workflow:**
```yaml
- name: Sync to Aliyun OSS
  uses: xcube-studio/sync2oss@v2
  with:
    accessKeyId: ${{ secrets.OSS_ACCESS_KEY_ID }}
    accessKeySecret: ${{ secrets.OSS_ACCESS_KEY_SECRET }}
    endpoint: oss-cn-example.aliyuncs.com
    bucketName: my-bucket
    fromRelease: true
    repoUrl: yourusername/repo
    isPre: false
    keepCount: 2
    addSymlink: false
    region: cn-shanghai
```
> 更多参数说明请查阅 [`action.yml`](./action.yml)。

### 2. 命令行方式使用

你也可以在本地或 CI 环境中直接构建并运行 `src` 目录下的可执行文件。

**构建命令：**
```bash
cd src
dotnet build -c Release -o build
```

**运行示例：**
```bash
# Windows
build\Sync2Oss.exe --accessKeyId xxx --accessKeySecret xxx --endpoint xxx --bucketName xxx --fromRelease true --repoUrl yourusername/repo

# Linux/macOS
chmod +x build/Sync2Oss
./build/Sync2Oss --accessKeyId xxx --accessKeySecret xxx --endpoint xxx --bucketName xxx --fromRelease true --repoUrl yourusername/repo
```

## 使用方式说明

Sync2OSS 支持两种上传方式：

- **fromRelease 为 True**：自动拉取并上传 GitHub 仓库的最新 Release 资源，无需指定 localPath 和 remoteDir。
- **fromRelease 为 False**：根据 localPath 和 remoteDir 参数，将本地指定文件/文件夹上传到 OSS 的指定目录。

请根据实际需求选择合适的参数组合。

## 参数说明

| 参数名           | 说明                         | 是否必需 | 默认值   |
|------------------|------------------------------|----------|----------|
| accessKeyId      | 阿里云 AccessKeyId           | 是       | -        |
| accessKeySecret  | 阿里云 AccessKeySecret       | 是       | -        |
| endpoint         | 阿里云 Endpoint               | 是       | -        |
| bucketName       | 阿里云 BucketName             | 是       | -        |
| fromRelease      | 是否从 Release 上传           | 否       | true     |
| repoUrl          | GitHub 仓库地址               | 否       | -        |
| isPre            | 是否上传 PreRelease           | 否       | false    |
| keepCount        | 保留最新几个版本              | 否       | 2        |
| remoteDir        | 上传到远程目录                | 否       | -        |
| addSymlink       | 是否添加软链接                | 否       | false    |
| localPath        | 本地文件/文件夹路径           | 否       | -        |
| region           | 阿里云 OSS 区域               | 否       | cn-shanghai |

> 详细参数说明请参考 action.yml 或命令行 --help。

## 许可协议

MIT License