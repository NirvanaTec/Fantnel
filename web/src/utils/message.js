// 消息提示功能

// 消息类型配置
const messageTypes = {
  success: {
    backgroundColor: '#4CAF50'
  },
  error: {
    backgroundColor: '#F44336'
  },
  info: {
    backgroundColor: '#2196F3'
  },
  warning: {
    backgroundColor: '#FFC107',
    color: '#333'
  }
}

// 显示消息
const showMessage = (content, type = 'info', duration = 3000) => {
  // 创建消息容器
  const messageContainer = document.createElement('div')
  messageContainer.className = `message message-${type}`
  messageContainer.textContent = content
  
  // 设置样式
  Object.assign(messageContainer.style, {
    position: 'fixed',
    top: '20px',
    right: '20px',
    padding: '12px 20px',
    borderRadius: '8px',
    color: 'white',
    fontSize: '14px',
    fontWeight: '500',
    boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
    opacity: '0',
    transform: 'translateY(-20px)',
    transition: 'all 0.3s ease',
    zIndex: '9999',
    ...messageTypes[type]
  })
  
  // 添加到页面
  document.body.appendChild(messageContainer)
  
  // 显示动画
  setTimeout(() => {
    messageContainer.style.opacity = '1'
    messageContainer.style.transform = 'translateY(0)'
  }, 10)
  
  // 自动隐藏
  setTimeout(() => {
    messageContainer.style.opacity = '0'
    messageContainer.style.transform = 'translateY(-20px)'
    setTimeout(() => {
      document.body.removeChild(messageContainer)
    }, 300)
  }, duration)
}

// 导出消息方法
export const Message = {
  success: (content, duration) => showMessage(content, 'success', duration),
  error: (content, duration) => showMessage(content, 'error', duration),
  info: (content, duration) => showMessage(content, 'info', duration),
  warning: (content, duration) => showMessage(content, 'warning', duration)
}

export default Message
