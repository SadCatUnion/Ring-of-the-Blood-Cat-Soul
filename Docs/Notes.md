## 摄像机模式
freecam/lockon

## freecam animation
### 旋转
```
TargetDirection = (Input.X, Input.Y).Normalized
if TargetDirection - CurrentDirection == 180 then
    if IsSprint then
        Sprint Turn 180
    end
end

CurrentDirection = (TargetDirection - CurrentDirection) by AngularSpeed
```
### 速度
```
Speed = Input.Normalized.Length
if Input.Sprint then
    Speed = Speed + x
end
if Speed > v1 then
    Sprint
elseif Speed > v2 then
    Run
elseif Speed > v3 then
    Walk
elseif Speed > v4 then
    Idle
end
```
## Conference Link
[参考教程](https://www.bilibili.com/video/BV1m5411J7ci?p=6)