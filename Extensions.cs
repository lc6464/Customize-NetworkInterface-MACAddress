namespace 自定义网卡硬件地址 {
	static class IEnumerableExtensions {
		/// <summary>
		/// 判断 <paramref name="enumerable"/> 是否存在元素
		/// </summary>
		/// <typeparam name="T"><paramref name="enumerable"/> 的类型</typeparam>
		/// <param name="enumerable">被判断是否含有元素的 <see cref="IEnumerable{T}"/></param>
		/// <returns></returns>
		public static bool HasValue<T>(this IEnumerable<T>? enumerable) {
			return enumerable != null && enumerable.Any();
		}

		/// <summary>
		/// 判断 <paramref name="enumerable"/> 是否存在指定元素
		/// </summary>
		/// <typeparam name="T"><paramref name="enumerable"/> 的类型</typeparam>
		/// <param name="enumerable">被判断是否含有指定元素的 <see cref="IEnumerable{T}"/></param>
		/// <param name="value">指定元素</param>
		/// <returns></returns>
		public static bool HasValue<T>(this IEnumerable<T>? enumerable, T value) {
			if (enumerable != null && enumerable.Any()) {
				foreach (T item in enumerable) {
					if (item?.Equals(value) ?? false) {
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <summary>
	/// string 的扩展方法。
	/// </summary>
	static class StringExtensions {
		/// <summary>
		/// 获取判断性字符串（Y/N）指示的值，默认值默认为 No。
		/// </summary>
		/// <param name="s">输入字符串</param>
		/// <param name="defaultIsYesOrNo">默认值，Yes 为 <see cref="true"/>, No 为 <see cref="false"/>, 默认为 No.</param>
		/// <returns>Yes 则返回 <see cref="true"/>, No 则返回 <see cref="false"/>. <paramref name="s"/> 为空则直接返回默认值 <paramref name="defaultIsYesOrNo"/>，否则判断是否是默认值相反的结果，如果是，则返回默认值相反的结果，否则返回默认值。</returns>
		public static bool GetIsYesOrNo(this string? s, bool defaultIsYesOrNo = false) {
			if (string.IsNullOrWhiteSpace(s)) { // 如果为 null、空或空白字符，直接返回默认值
				return defaultIsYesOrNo;
			} else if (defaultIsYesOrNo) { // 默认为 Yes，则判断是否为 No
				if (string.Equals(s, "no", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(s, "n", StringComparison.OrdinalIgnoreCase)
					|| s == "否") {
					return false; // 若为 No，则返回 false
				} else {
					return true; // 若不为 No，则返回默认值 Yes（true)
				}
			} else { // 默认为 No，则判断是否为 Yes
				if (string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(s, "y", StringComparison.OrdinalIgnoreCase)
					|| s == "是") {
					return true; // 若为 No，则返回 false
				} else {
					return false; // 若不为 No，则返回默认值 Yes（true)
				}
			}
		}

		public static bool TryExtractMacAddress(this string? value, out string macAddress) {
			if (!string.IsNullOrEmpty(value)) {
				Match match = Statics.macRegex.Match(value.Trim().ToUpper());
				if (match.Success) {
					macAddress = match.Groups[0].Value;
					if (match.Groups[2].Length > 0) {
						macAddress = macAddress.Replace(":", "");
					} else if (match.Groups[3].Length > 0) {
						macAddress = macAddress.Replace("-", "");
					}
					return true;
				}
			}
			macAddress = string.Empty;
			return false;
		}
	}
}