<template>
	<div class="modal-backdrop">
		<div class="modal">
			<div class="modal-header">
				<h2>{{ title }}</h2>
				<button @click="$emit('cancel')" class="close-button">
					<i class="fas fa-times"></i>
				</button>
			</div>
			<div class="modal-body">
				<p class="confirmation-message">{{ message }}</p>

				<div class="form-actions">
					<button
						type="button"
						class="cancel-button"
						@click="$emit('cancel')"
					>
						Hủy
					</button>
					<button
						type="button"
						class="confirm-button"
						@click="$emit('confirm')"
					>
						Xác nhận
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
defineProps({
	title: {
		type: String,
		default: "Xác nhận",
	},
	message: {
		type: String,
		required: true,
	},
});

defineEmits(["confirm", "cancel"]);
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
	width: 400px;
	max-width: 95%;
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

.confirmation-message {
	margin: 0 0 1.5rem 0;
	line-height: 1.5;
	color: #555;
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	gap: 1rem;
	position: relative;
	z-index: 5;
}

.cancel-button,
.confirm-button {
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

.confirm-button {
	background-color: #ff4757;
	color: white;
	border: none;
	position: relative;
	z-index: 10;
	min-width: 100px;
	display: flex;
	align-items: center;
	justify-content: center;
}

.confirm-button:hover {
	background-color: #ff2c41;
}
</style>
