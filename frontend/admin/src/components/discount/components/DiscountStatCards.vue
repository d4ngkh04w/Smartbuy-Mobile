<template>
	<div class="stat-cards">
		<div class="stat-card">
			<div class="stat-icon total">
				<i class="fas fa-tags"></i>
			</div>
			<div class="stat-content">
				<p class="stat-title">Tổng số mã giảm giá</p>
				<h3 class="stat-value">{{ totalDiscounts }}</h3>
			</div>
		</div>
		<div class="stat-card">
			<div class="stat-icon active">
				<i class="fas fa-check-circle"></i>
			</div>
			<div class="stat-content">
				<p class="stat-title">Mã giảm giá đang hoạt động</p>
				<h3 class="stat-value">{{ activeDiscounts }}</h3>
			</div>
		</div>
		<div class="stat-card">
			<div class="stat-icon upcoming">
				<i class="fas fa-calendar-alt"></i>
			</div>
			<div class="stat-content">
				<p class="stat-title">Mã giảm giá sắp tới</p>
				<h3 class="stat-value">{{ upcomingDiscounts }}</h3>
			</div>
		</div>
	</div>
</template>

<script setup>
defineProps({
	totalDiscounts: {
		type: Number,
		default: 0,
	},
	activeDiscounts: {
		type: Number,
		default: 0,
	},
	upcomingDiscounts: {
		type: Number,
		default: 0,
	},
});
</script>

<style scoped>
.stat-cards {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
	gap: 1.5rem;
	margin-bottom: 1.5rem;
}

.stat-card {
	background-color: white;
	border-radius: 12px;
	padding: 1.5rem;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
	display: flex;
	align-items: center;
	gap: 1rem;
	transition: all 0.2s;
}

.stat-card:hover {
	transform: translateY(-3px);
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.stat-icon {
	width: 50px;
	height: 50px;
	border-radius: 12px;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 1.5rem;
}

.stat-icon.total {
	background-color: rgba(var(--primary-rgb), 0.15);
	color: var(--primary-color);
}

.stat-icon.active {
	background-color: rgba(46, 213, 115, 0.15);
	color: #2ed573;
}

.stat-icon.upcoming {
	background-color: rgba(28, 126, 214, 0.15);
	color: #1c7ed6;
}

.stat-content {
	flex: 1;
}

.stat-title {
	font-size: 0.85rem;
	color: #777;
	margin: 0 0 0.25rem 0;
}

.stat-value {
	font-size: 1.5rem;
	font-weight: 600;
	color: #333;
	margin: 0;
}
</style>
