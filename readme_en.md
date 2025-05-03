# Vfiletransfer

A focused GitHub Action / .NET CLI for:

- uploading a single local file to Alibaba Cloud OSS
- working with private read-write buckets
- generating a signed download URL valid for one hour
- exposing that URL through GitHub Actions outputs for downstream steps

## Inputs

| Input | Required | Default | Description |
| --- | --- | --- | --- |
| `accessKeyId` | Yes | - | Aliyun AccessKeyId |
| `accessKeySecret` | Yes | - | Aliyun AccessKeySecret |
| `endpoint` | Yes | - | OSS endpoint, for example `oss-cn-shanghai.aliyuncs.com` |
| `bucketName` | Yes | - | OSS bucket name |
| `localPath` | Yes | - | Local file path to upload; single file only |
| `objectKey` | No | file name | Target OSS object key |
| `region` | No | `cn-shanghai` | OSS region |
| `expiresInSeconds` | No | `3600` | Signed URL lifetime in seconds |
| `overwrite` | No | `true` | Whether to overwrite an existing remote object |

## Outputs

| Output | Description |
| --- | --- |
| `downloadUrl` | Signed download URL |
| `signedUrl` | Alias of `downloadUrl` |
| `objectKey` | Uploaded OSS object key |
| `ossUri` | Uploaded OSS URI, such as `oss://bucket/path/file.zip` |

## Workflow example

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

## CLI example

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

## Notes

- If `endpoint` is provided without a scheme, the tool upgrades it to `https://`
- If `objectKey` is omitted, the local file name is used
- This branch is now a dedicated `Vfiletransfer` implementation and no longer preserves the older release-sync workflow
