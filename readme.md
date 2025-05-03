# Vfiletransfer

一个专用的 GitHub Action / .NET CLI：

- 上传单个本地文件到阿里云 OSS
- 适用于私有读写 Bucket
- 生成 1 小时有效的签名下载链接
- 把链接写入 GitHub Actions `output`，方便后续步骤直接消费

## 输入参数

| 参数 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `accessKeyId` | 是 | - | 阿里云 AccessKeyId |
| `accessKeySecret` | 是 | - | 阿里云 AccessKeySecret |
| `endpoint` | 是 | - | OSS Endpoint，例如 `oss-cn-shanghai.aliyuncs.com` |
| `bucketName` | 是 | - | OSS Bucket 名称 |
| `localPath` | 是 | - | 要上传的本地文件路径，仅支持单文件 |
| `objectKey` | 否 | 文件名 | OSS 中的目标对象 Key |
| `region` | 否 | `cn-shanghai` | OSS 区域 |
| `expiresInSeconds` | 否 | `3600` | 签名下载链接有效期，单位秒 |
| `overwrite` | 否 | `true` | 远程对象已存在时是否覆盖 |

## 输出参数

| 输出 | 说明 |
| --- | --- |
| `downloadUrl` | 签名下载链接 |
| `signedUrl` | `downloadUrl` 的别名 |
| `objectKey` | 上传后的 OSS 对象 Key |
| `ossUri` | 上传后的 OSS 地址，例如 `oss://bucket/path/file.zip` |

## Workflow 示例

```yaml
jobs:
  transfer:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Build artifact
        run: echo "hello" > artifact.txt

      - name: Upload to private OSS
        id: vfiletransfer
        uses: xcube-studio/sync2oss@Vfiletransfer
        with:
          accessKeyId: ${{ secrets.OSS_ACCESS_KEY_ID }}
          accessKeySecret: ${{ secrets.OSS_ACCESS_KEY_SECRET }}
          endpoint: oss-cn-shanghai.aliyuncs.com
          bucketName: your-private-bucket
          localPath: ./artifact.txt
          objectKey: ci/artifact-${{ github.run_id }}.txt
          region: cn-shanghai
          expiresInSeconds: 3600

      - name: Use output in later steps
        run: |
          echo "download url: ${{ steps.vfiletransfer.outputs.downloadUrl }}"
          echo "oss key: ${{ steps.vfiletransfer.outputs.objectKey }}"
```

## 命令行示例

```bash
cd src
dotnet build -c Release
dotnet run -- \
  --accessKeyId xxx \
  --accessKeySecret xxx \
  --endpoint oss-cn-shanghai.aliyuncs.com \
  --bucketName your-private-bucket \
  --localPath ./artifact.txt \
  --objectKey ci/artifact.txt \
  --region cn-shanghai \
  --expiresInSeconds 3600 \
  --overwrite true
```

## 说明

- 如果 `endpoint` 没带协议头，程序会自动补成 `https://`
- 如果未传 `objectKey`，默认使用本地文件名
- 当前版本是为 `Vfiletransfer` 分支定制的专用实现，不再兼容旧的 Release 同步模式
