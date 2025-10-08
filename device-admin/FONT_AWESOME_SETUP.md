# Font Awesome 图标设置指南

## 安装 Font Awesome

### 方法1：使用CDN（推荐，简单快速）

在 `device-admin/index.html` 中添加以下代码到 `<head>` 部分：

```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" 
      integrity="sha512-iecdLmaskl7CVkqkXNQ/ZH/XLlvWZOJyj7Yy7tcenmpD1ypASozpmT/E0iPtmFIB46ZmdtAc9eNBvH0H/ZpiBw==" 
      crossorigin="anonymous" referrerpolicy="no-referrer" />
```

### 方法2：使用npm安装

```bash
cd device-admin
npm install @fortawesome/fontawesome-free
```

然后在 `main.ts` 中导入：

```typescript
import '@fortawesome/fontawesome-free/css/all.min.css'
```

## 图标使用

现在可以在Vue组件中使用Font Awesome图标：

```html
<!-- 查看详情 -->
<i class="fas fa-eye"></i>

<!-- 编辑用户 -->
<i class="fas fa-edit"></i>

<!-- 重置密码 -->
<i class="fas fa-key"></i>

<!-- 删除用户 -->
<i class="fas fa-trash"></i>
```

## 可用的图标类

### 操作图标
- `fa-eye` - 查看
- `fa-edit` - 编辑
- `fa-key` - 密码/钥匙
- `fa-trash` - 删除
- `fa-plus` - 添加
- `fa-search` - 搜索
- `fa-refresh` - 刷新
- `fa-save` - 保存
- `fa-cancel` - 取消

### 状态图标
- `fa-check` - 成功
- `fa-times` - 错误
- `fa-warning` - 警告
- `fa-info` - 信息
- `fa-lock` - 锁定
- `fa-unlock` - 解锁

## CSS样式

```css
.btn-icon i {
  font-size: 14px;
  line-height: 1;
  display: block;
}

.btn-icon:hover i {
  color: #374151 !important;
}

.btn-icon.danger i {
  color: #dc2626 !important;
}

.btn-icon.danger:hover i {
  color: #b91c1c !important;
}
```

## 优势

1. **美观**：专业的矢量图标，清晰美观
2. **一致**：所有图标风格统一
3. **丰富**：提供大量图标选择
4. **兼容**：支持所有现代浏览器
5. **可定制**：可以调整大小、颜色等

## 测试

安装完成后，按钮应该显示美观的Font Awesome图标，不再是Unicode字符。
