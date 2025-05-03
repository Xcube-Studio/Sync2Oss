# Sync2OSS

## Introduction

Sync2OSS is a tool for synchronizing files to Alibaba Cloud OSS, supporting both GitHub Actions and command-line usage. It is developed with .NET 9 and is ideal for automated publishing, backup, and similar scenarios. Currently, only Alibaba Cloud OSS is supported as the object storage provider.

## Features

- Sync files to Alibaba Cloud OSS via GitHub Actions
- Flexible command-line usage for uploading local files/folders or GitHub Release assets
- Symlink support
- Customizable version retention, target directory, and more
- Detailed logging for troubleshooting

## Versioning

- **v2**: Always tracks the latest HEAD(v2), suitable for users who want the newest features and fixes.
- **Release Tag**: Check the Release page for specific tags to use a stable, fixed version.

> For production, it is recommended to use a Release Tag. For development or trying new features, use v2.

## Usage Modes

Sync2OSS supports two upload modes:

- **fromRelease = True**: Automatically fetches and uploads the latest Release assets from the GitHub repository. No need to specify localPath and remoteDir.
- **fromRelease = False**: Uploads local files/folders specified by localPath to the OSS directory specified by remoteDir.

Choose the appropriate parameter combination according to your needs.

## Usage

### 1. As a GitHub Action

You can use this project directly in your workflow. For detailed parameters and usage, please refer to [`action.yml`](./action.yml).

**Example workflow:**
```yaml
- name: Sync to Aliyun OSS
  uses: xcube-studio/sync2oss@v2
  with:
    accessKeyId: ${{ secrets.OSS_ACCESS_KEY_ID }}
    accessKeySecret: ${{ secrets.OSS_ACCESS_KEY_SECRET }}
    endpoint: oss-cn-shanghai.aliyuncs.com
    bucketName: my-bucket
    fromRelease: true
    repoUrl: xcube-studio/sync2oss
    isPre: false
    keepCount: 2
    remoteDir: my-dir
    addSymlink: false
    localPath: ./dist
    region: cn-shanghai
```
> For more parameter details, see [`action.yml`](./action.yml).

### 2. Command Line Usage

You can also build and run the executable in the `src` directory locally or in your CI environment.

**Build:**
```bash
cd src
dotnet build -c Release -o build
```

**Run example:**
```bash
# Windows
build\Sync2Oss.exe --accessKeyId xxx --accessKeySecret xxx --endpoint xxx --bucketName xxx --fromRelease true --repoUrl xcube-studio/sync2oss

# Linux/macOS
chmod +x build/Sync2Oss
./build/Sync2Oss --accessKeyId xxx --accessKeySecret xxx --endpoint xxx --bucketName xxx --fromRelease true --repoUrl xcube-studio/sync2oss
```

## Parameter Table

| Name            | Description                                 | Required | Default      |
|-----------------|---------------------------------------------|----------|--------------|
| accessKeyId     | Aliyun AccessKeyId                          | Yes      | -            |
| accessKeySecret | Aliyun AccessKeySecret                      | Yes      | -            |
| endpoint        | Aliyun Endpoint                             | Yes      | -            |
| bucketName      | Aliyun BucketName                           | Yes      | -            |
| fromRelease     | Upload from GitHub Release                  | No       | true         |
| repoUrl         | GitHub repository url                       | No       | -            |
| isPre           | Upload PreRelease                           | No       | false        |
| keepCount       | Number of versions to keep                  | No       | 2            |
| remoteDir       | Target directory on OSS                     | No       | -            |
| addSymlink      | Add symlink                                 | No       | false        |
| localPath       | Local file/folder path                      | No       | -            |
| region          | Aliyun OSS region                           | No       | cn-shanghai  |

> For details, see action.yml or use --help in CLI.

## License

MIT License
