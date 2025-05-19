<template>
    <div class="overview-content">
        <!-- Membership card -->
        <div class="membership-card">
            <div class="membership-header">
                <div class="membership-icon">
                    <i class="fas fa-medal"></i>
                </div>
                <div class="membership-title">
                    <h2>Thành viên Silver</h2>
                </div>
            </div>

            <div class="membership-progress">
                <div class="membership-levels">
                    <div class="level edu">Edu</div>
                    <div class="level new">New</div>
                    <div class="level silver active">Silver</div>
                    <div class="level gold">Gold</div>
                    <div class="level diamond">Diamond</div>
                </div>
                <div class="progress-bar">
                    <div class="progress-fill silver" style="width: 60%"></div>
                </div>
            </div>

            <div class="membership-details">
                <p>Tổng chi tiêu: <strong>2,346,000đ</strong></p>
                <p>
                    Thứ hạng được cập nhật lúc:
                    <strong>17/03/2025</strong>
                </p>
            </div>

            <div class="membership-upgrade-info">
                <div class="info-box">
                    <i class="fas fa-star"></i>
                    <p>
                        Bạn là Học sinh - Sinh viên - Giáo viên?
                        <strong>Đăng ký thành viên Edu ngay</strong>
                    </p>
                    <i class="fas fa-arrow-right"></i>
                </div>
            </div>
        </div>

        <!-- Membership benefits -->
        <div class="section-title">Khám phá ưu đãi hạng thành viên</div>
        <div class="membership-tiers">
            <div class="tier-card">
                <div class="tier-icon edu">
                    <!-- <img src="" alt="Edu" /> -->
                    <span>Edu</span>
                </div>
            </div>
            <div class="tier-card">
                <div class="tier-icon new">
                    <!-- <img src="" alt="New" /> -->
                    <span>New</span>
                </div>
            </div>
            <div class="tier-card active">
                <div class="tier-icon silver">
                    <!-- <img src="" alt="Silver" /> -->
                    <span>Silver</span>
                    <div class="tier-check">
                        <i class="fas fa-check-circle"></i>
                    </div>
                </div>
            </div>
            <div class="tier-card">
                <div class="tier-icon gold">
                    <!-- <img src="" alt="Gold" /> -->
                    <span>Gold</span>
                </div>
            </div>
            <div class="tier-card">
                <div class="tier-icon diamond">
                    <!-- <img src="" alt="Diamond" /> -->
                    <span>Diamond</span>
                </div>
            </div>
        </div>

        <!-- Membership benefits details -->
        <div class="benefits-detail">
            <div class="benefit-section">
                <div class="benefit-icon">
                    <i class="fas fa-award"></i>
                </div>
                <div class="benefit-content">
                    <h3>Điều kiện</h3>
                    <p>
                        Tổng số tiền mua hàng tích lũy trong thời gian đạt dưới
                        30 triệu đồng.
                    </p>
                </div>
            </div>

            <div class="benefit-section">
                <div class="benefit-icon">
                    <i class="fas fa-percentage"></i>
                </div>
                <div class="benefit-content">
                    <h3>
                        Nguyên tắc tích điểm (nguyên tắc chung áp dụng với tất
                        cả ngành hàng)
                    </h3>
                    <p>0,3% trên giá trị đơn hàng mới</p>
                </div>
            </div>

            <div class="benefit-section">
                <div class="benefit-icon">
                    <i class="fas fa-tags"></i>
                </div>
                <div class="benefit-content">
                    <h3>Nguyên tắc chiết khấu</h3>
                    <p>0,4%</p>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
// No props or emits needed for this component as it's static content
</script>

<style scoped>
.overview-content {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.membership-card {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.membership-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.membership-icon {
    width: 50px;
    height: 50px;
    background: linear-gradient(135deg, #c0c0c0, #e8e8e8);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.membership-icon i {
    font-size: 1.5rem;
    color: #333;
}

.membership-title h2 {
    margin: 0;
    font-size: 1.5rem;
    color: #333;
}

.membership-progress {
    margin-bottom: 1.5rem;
}

.membership-levels {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.5rem;
}

.level {
    font-size: 0.85rem;
    color: #777;
}

.level.active {
    color: var(--primary-color);
    font-weight: 600;
}

.progress-bar {
    height: 8px;
    background-color: #eee;
    border-radius: 4px;
    overflow: hidden;
}

.progress-fill {
    height: 100%;
    background: linear-gradient(90deg, #c0c0c0, #a0a0a0);
    border-radius: 4px;
}

.progress-fill.silver {
    background: linear-gradient(90deg, #c0c0c0, #a0a0a0);
}

.membership-details {
    margin-bottom: 1.5rem;
}

.membership-details p {
    margin: 0.5rem 0;
    color: #666;
}

.membership-details strong {
    color: #333;
    font-weight: 600;
}

.membership-upgrade-info {
    background-color: #f9f0ff;
    border-radius: 8px;
    padding: 1rem;
    border-left: 3px solid var(--primary-color);
}

.info-box {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.info-box i {
    color: var(--primary-color);
}

.info-box p {
    margin: 0;
    flex: 1;
    font-size: 0.9rem;
    color: #555;
}

.info-box strong {
    color: var(--primary-color);
}

/* Membership tiers */
.section-title {
    font-size: 1.25rem;
    font-weight: 600;
    color: #333;
    margin: 1rem 0;
}

.membership-tiers {
    display: flex;
    gap: 1rem;
    overflow-x: auto;
    padding: 0.5rem 0;
}

.tier-card {
    flex: 0 0 100px;
    background-color: white;
    border-radius: 8px;
    padding: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    text-align: center;
    transition: all 0.3s ease;
    position: relative;
}

.tier-card.active {
    box-shadow: 0 4px 16px rgba(248, 110, 211, 0.25);
    transform: translateY(-4px);
    border: 1px solid var(--primary-color);
}

.tier-icon {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    margin: 0 auto 0.75rem;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
}

.tier-icon img {
    width: 30px;
    height: 30px;
    object-fit: contain;
    margin-bottom: 0.25rem;
}

.tier-icon span {
    font-size: 0.8rem;
    font-weight: 600;
}

.tier-icon.edu {
    background-color: #e3f2fd;
    color: #1976d2;
}

.tier-icon.new {
    background-color: #e8f5e9;
    color: #388e3c;
}

.tier-icon.silver {
    background-color: #f3f3f3;
    color: #757575;
}

.tier-icon.gold {
    background-color: #fff8e1;
    color: #ffa000;
}

.tier-icon.diamond {
    background-color: #e1f5fe;
    color: #0288d1;
}

.tier-check {
    position: absolute;
    top: -5px;
    right: -5px;
    width: 20px;
    height: 20px;
    background-color: var(--primary-color);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.tier-check i {
    font-size: 0.7rem;
    color: white;
}

/* Benefits detail */
.benefits-detail {
    background-color: white;
    border-radius: 10px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.benefit-section {
    display: flex;
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.benefit-section:last-child {
    margin-bottom: 0;
}

.benefit-icon {
    width: 40px;
    height: 40px;
    background-color: #f9f0ff;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--primary-color);
    flex-shrink: 0;
}

.benefit-content {
    flex: 1;
}

.benefit-content h3 {
    margin: 0 0 0.5rem;
    font-size: 1rem;
    color: #333;
}

.benefit-content p {
    margin: 0;
    color: #666;
    font-size: 0.95rem;
}
</style>
