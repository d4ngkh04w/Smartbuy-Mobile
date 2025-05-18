<template>
	<div class="modal-backdrop">
		<div class="modal">
			<div class="modal-header">
				<h2>Thêm mã giảm giá mới</h2>
				<button @click="$emit('close')" class="close-button">
					<i class="fas fa-times"></i>
				</button>
			</div>
			<div class="modal-body">
				<form @submit.prevent="handleSubmit">
					<div class="discount-type-selector">
						<div class="form-group">
							<label>Loại giảm giá</label>
							<div class="discount-type-options">
								<div
									class="discount-type-option"
									:class="{
										active: discountType === 'percentage',
									}"
									@click="setDiscountType('percentage')"
								>
									<i class="fas fa-percentage"></i>
									<span>Phần trăm</span>
								</div>
								<div
									class="discount-type-option"
									:class="{
										active: discountType === 'amount',
									}"
									@click="setDiscountType('amount')"
								>
									<i class="fas fa-money-bill-wave"></i>
									<span>Số tiền</span>
								</div>
							</div>
						</div>
					</div>

					<div
						class="form-group"
						v-if="discountType === 'percentage'"
					>
						<label for="discountPercentage"
							>Phần trăm giảm giá (%)</label
						>
						<input
							id="discountPercentage"
							v-model="discountForm.discountPercentage"
							type="number"
							min="0"
							max="100"
							step="0.01"
							placeholder="Nhập % giảm giá"
							:class="{ 'has-error': errors.discountPercentage }"
						/>
						<p v-if="errors.discountPercentage" class="error-text">
							{{ errors.discountPercentage }}
						</p>
					</div>

					<div class="form-group" v-if="discountType === 'amount'">
						<label for="discountAmount">Số tiền giảm (VNĐ)</label>
						<input
							id="discountAmount"
							v-model="discountForm.discountAmount"
							type="number"
							min="0"
							step="1000"
							placeholder="Nhập số tiền giảm"
							:class="{ 'has-error': errors.discountAmount }"
						/>
						<p v-if="errors.discountAmount" class="error-text">
							{{ errors.discountAmount }}
						</p>
					</div>

					<div class="form-row">
						<div class="form-group">
							<label for="startDate">Ngày bắt đầu</label>
							<input
								id="startDate"
								v-model="discountForm.startDate"
								type="datetime-local"
								:class="{ 'has-error': errors.startDate }"
							/>
							<p v-if="errors.startDate" class="error-text">
								{{ errors.startDate }}
							</p>
						</div>

						<div class="form-group">
							<label for="endDate">Ngày kết thúc</label>
							<input
								id="endDate"
								v-model="discountForm.endDate"
								type="datetime-local"
								:class="{ 'has-error': errors.endDate }"
							/>
							<p v-if="errors.endDate" class="error-text">
								{{ errors.endDate }}
							</p>
						</div>
					</div>

					<div class="form-actions">
						<button
							type="button"
							class="cancel-button"
							@click="$emit('close')"
							:disabled="loading"
						>
							Hủy bỏ
						</button>
						<button
							type="submit"
							class="submit-button"
							:disabled="loading"
						>
							<span v-if="loading" class="spinner"></span>
							<span v-else>Thêm mới</span>
						</button>
					</div>
				</form>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, reactive, watch } from "vue";

const emit = defineEmits(["close", "add"]);

const loading = ref(false);
const discountType = ref("percentage"); // Default to percentage discount

const discountForm = reactive({
	discountPercentage: 0,
	discountAmount: 0,
	startDate: formatDateForInput(new Date()),
	endDate: formatDateForInput(getDefaultEndDate()),
});

const errors = reactive({
	discountPercentage: "",
	discountAmount: "",
	startDate: "",
	endDate: "",
});

// When changing discount type, reset the value of the other type
function setDiscountType(type) {
	discountType.value = type;
	if (type === "percentage") {
		discountForm.discountAmount = 0;
	} else {
		discountForm.discountPercentage = 0;
	}
}

// Watch for changes in discount values to ensure only one type is set
watch(
	() => discountForm.discountPercentage,
	(newVal) => {
		if (newVal > 0) {
			discountType.value = "percentage";
			discountForm.discountAmount = 0;
		}
	}
);

watch(
	() => discountForm.discountAmount,
	(newVal) => {
		if (newVal > 0) {
			discountType.value = "amount";
			discountForm.discountPercentage = 0;
		}
	}
);

function formatDateForInput(date) {
	const d = new Date(date);
	const year = d.getFullYear();
	const month = String(d.getMonth() + 1).padStart(2, "0");
	const day = String(d.getDate()).padStart(2, "0");
	const hours = String(d.getHours()).padStart(2, "0");
	const minutes = String(d.getMinutes()).padStart(2, "0");

	return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function getDefaultEndDate() {
	const date = new Date();
	date.setDate(date.getDate() + 30); // Default to 30 days from now
	return date;
}

function validateForm() {
	// Reset errors
	Object.keys(errors).forEach((key) => (errors[key] = ""));
	let isValid = true;

	// Check if at least one discount type is provided
	if (discountType.value === "percentage") {
		if (discountForm.discountPercentage <= 0) {
			errors.discountPercentage = "Phần trăm giảm giá phải lớn hơn 0";
			isValid = false;
		} else if (discountForm.discountPercentage > 100) {
			errors.discountPercentage =
				"Phần trăm giảm giá phải từ 0% đến 100%";
			isValid = false;
		}
	} else {
		if (discountForm.discountAmount <= 0) {
			errors.discountAmount = "Số tiền giảm phải lớn hơn 0";
			isValid = false;
		}
	}

	// Validate dates
	if (!discountForm.startDate) {
		errors.startDate = "Vui lòng chọn ngày bắt đầu";
		isValid = false;
	}

	if (!discountForm.endDate) {
		errors.endDate = "Vui lòng chọn ngày kết thúc";
		isValid = false;
	}

	// Validate end date is after start date
	if (discountForm.startDate && discountForm.endDate) {
		const start = new Date(discountForm.startDate);
		const end = new Date(discountForm.endDate);

		if (end <= start) {
			errors.endDate = "Ngày kết thúc phải sau ngày bắt đầu";
			isValid = false;
		}
	}

	return isValid;
}

async function handleSubmit() {
	if (!validateForm()) return;

	loading.value = true;

	try {
		const formData = {
			discountPercentage:
				parseFloat(discountForm.discountPercentage) || 0,
			discountAmount: parseFloat(discountForm.discountAmount) || 0,
			startDate: new Date(discountForm.startDate).toISOString(),
			endDate: new Date(discountForm.endDate).toISOString(),
		};

		emit("add", formData);
	} catch (error) {
		console.error("Error submitting form", error);
	} finally {
		loading.value = false;
	}
}
</script>

<style scoped>
.modal-backdrop {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background-color: rgba(0, 0, 0, 0.5);
	display: flex;
	justify-content: center;
	align-items: center;
	z-index: 1000;
}

.modal {
	background-color: white;
	border-radius: 12px;
	width: 500px;
	max-width: 95%;
	max-height: 90vh;
	overflow-y: auto;
	box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
	animation: slide-in 0.3s ease;
}

@keyframes slide-in {
	from {
		transform: translateY(-30px);
		opacity: 0;
	}
	to {
		transform: translateY(0);
		opacity: 1;
	}
}

.modal-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 1.5rem;
	border-bottom: 1px solid #eee;
}

.modal-header h2 {
	margin: 0;
	font-size: 1.3rem;
	color: #333;
}

.close-button {
	background-color: rgba(153, 153, 153, 0.1);
	border: none;
	font-size: 0.9rem;
	color: #999;
	cursor: pointer;
	transition: all 0.2s;
	width: 30px;
	height: 30px;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
	position: relative;
	z-index: 5;
}

.close-button:hover {
	background-color: #999;
	color: white;
	transform: scale(1.05);
}

.modal-body {
	padding: 1.5rem;
}

/* Discount type selector styling */
.discount-type-selector {
	margin-bottom: 1.5rem;
}

.discount-type-options {
	display: flex;
	gap: 1rem;
}

.discount-type-option {
	flex: 1;
	padding: 0.8rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	cursor: pointer;
	text-align: center;
	transition: all 0.2s ease;
}

.discount-type-option i {
	font-size: 1.2rem;
	margin-bottom: 0.5rem;
	display: block;
}

.discount-type-option:hover {
	border-color: var(--primary-color);
	background-color: rgba(var(--primary-rgb), 0.05);
}

.discount-type-option.active {
	border-color: var(--primary-color);
	background-color: rgba(var(--primary-rgb), 0.1);
	box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.1);
}

.form-group {
	margin-bottom: 1.2rem;
	display: flex;
	flex-direction: column;
}

.form-row {
	display: flex;
	gap: 1rem;
	margin-bottom: 1.2rem;
}

.form-row .form-group {
	flex: 1;
	margin-bottom: 0;
}

label {
	font-size: 0.9rem;
	font-weight: 500;
	margin-bottom: 0.5rem;
	color: #555;
}

input,
select,
textarea {
	padding: 0.6rem;
	border: 1px solid #ddd;
	border-radius: 8px;
	font-size: 0.95rem;
	transition: border-color 0.2s;
}

input:focus,
select:focus,
textarea:focus {
	outline: none;
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.1);
}

.has-error {
	border-color: #ff4757 !important;
	box-shadow: 0 0 0 2px rgba(255, 71, 87, 0.1);
}

.error-text {
	font-size: 0.8rem;
	color: #ff4757;
	margin: 0.3rem 0 0 0;
}

.help-text {
	font-size: 0.8rem;
	color: #888;
	margin: 0.3rem 0 0 0;
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
	margin-top: 1.5rem;
	position: relative;
	z-index: 5;
}

.cancel-button,
.submit-button {
	padding: 0.6rem 1.2rem;
	border-radius: 8px;
	font-size: 0.95rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
}

.cancel-button {
	background-color: #f1f1f1;
	color: #666;
	border: none;
}

.cancel-button:hover {
	background-color: #e5e5e5;
}

.submit-button {
	background-color: var(--primary-color);
	color: white;
	border: none;
	display: flex;
	align-items: center;
	justify-content: center;
	min-width: 100px;
	position: relative;
	z-index: 10;
}

.submit-button:hover {
	background-color: var(--primary-hover);
}

.submit-button:disabled,
.cancel-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
}

.spinner {
	width: 20px;
	height: 20px;
	border: 2px solid rgba(255, 255, 255, 0.3);
	border-radius: 50%;
	border-top-color: white;
	animation: spin 0.8s linear infinite;
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}
</style>
