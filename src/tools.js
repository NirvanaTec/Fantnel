import randomNameData from '../public/random.name.json'

// 生成随机游戏名称
export const generateRandomGameName = () => {
  const { prefixes, suffixes } = randomNameData
  let result, randomPrefix, randomSuffix, randomNumber

  do {
    randomPrefix = prefixes[Math.floor(Math.random() * prefixes.length)]
    randomSuffix = suffixes[Math.floor(Math.random() * suffixes.length)]
    randomNumber = ''

    // 先组合前缀和后缀
    let baseName = `${randomPrefix}${randomSuffix}`

    // 计算需要的随机数长度
    const currentLength = baseName.length
    if (currentLength < 7) {
      // 需要添加随机数，确保总长度在7-9之间
      const minNumbers = Math.max(0, 7 - currentLength)
      const maxNumbers = Math.max(0, 9 - currentLength)
      const numberLength = Math.floor(Math.random() * (maxNumbers - minNumbers + 1)) + minNumbers
      randomNumber = Math.floor(Math.random() * Math.pow(10, numberLength)).toString().padStart(numberLength, '0')
    } else if (currentLength > 9) {
      // 如果前缀+后缀已经超过9，需要重新选择
      continue
    }

    result = `${baseName}${randomNumber}`
  } while (result.length < 7 || result.length > 9)

  return result
}