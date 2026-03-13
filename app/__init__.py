from flask import Flask
from flask_sqlalchemy import SQLAlchemy
import os

db = SQLAlchemy()


def create_app(config=None):
    app = Flask(__name__)

    secret_key = os.environ.get('SECRET_KEY')
    if not secret_key:
        import warnings
        warnings.warn(
            "SECRET_KEY is not set. Using an insecure default. "
            "Set the SECRET_KEY environment variable in production.",
            stacklevel=2,
        )
        secret_key = 'thealley-hr-secret-key-2024'
    app.config['SECRET_KEY'] = secret_key
    app.config['SQLALCHEMY_DATABASE_URI'] = os.environ.get(
        'DATABASE_URL', 'sqlite:///thealley_hr.db'
    )
    app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False

    if config:
        app.config.update(config)

    db.init_app(app)

    from app.routes.main import main_bp
    from app.routes.employees import employees_bp
    from app.routes.departments import departments_bp
    from app.routes.positions import positions_bp
    from app.routes.attendance import attendance_bp
    from app.routes.leaves import leaves_bp
    from app.routes.payroll import payroll_bp

    app.register_blueprint(main_bp)
    app.register_blueprint(employees_bp, url_prefix='/nhan-vien')
    app.register_blueprint(departments_bp, url_prefix='/phong-ban')
    app.register_blueprint(positions_bp, url_prefix='/chuc-vu')
    app.register_blueprint(attendance_bp, url_prefix='/cham-cong')
    app.register_blueprint(leaves_bp, url_prefix='/nghi-phep')
    app.register_blueprint(payroll_bp, url_prefix='/luong')

    with app.app_context():
        db.create_all()
        _seed_data()

    return app


def _seed_data():
    from app.models import Department, Position, Employee
    from datetime import date

    if Department.query.count() > 0:
        return

    # Departments
    departments = [
        Department(name='Ban Giám Đốc', description='Ban lãnh đạo công ty'),
        Department(name='Kế Toán', description='Bộ phận kế toán tài chính'),
        Department(name='Nhân Sự', description='Bộ phận quản lý nhân sự'),
        Department(name='Pha Chế', description='Bộ phận pha chế đồ uống'),
        Department(name='Phục Vụ', description='Bộ phận phục vụ khách hàng'),
        Department(name='Kho Vận', description='Bộ phận quản lý kho và vận chuyển'),
    ]
    for d in departments:
        db.session.add(d)
    db.session.flush()

    dept_map = {d.name: d.id for d in departments}

    # Positions
    positions = [
        Position(name='Giám Đốc', department_id=dept_map['Ban Giám Đốc'],
                 base_salary=25000000),
        Position(name='Kế Toán Trưởng', department_id=dept_map['Kế Toán'],
                 base_salary=15000000),
        Position(name='Kế Toán Viên', department_id=dept_map['Kế Toán'],
                 base_salary=9000000),
        Position(name='Trưởng Phòng Nhân Sự', department_id=dept_map['Nhân Sự'],
                 base_salary=14000000),
        Position(name='Chuyên Viên Nhân Sự', department_id=dept_map['Nhân Sự'],
                 base_salary=9000000),
        Position(name='Trưởng Ca Pha Chế', department_id=dept_map['Pha Chế'],
                 base_salary=10000000),
        Position(name='Barista', department_id=dept_map['Pha Chế'],
                 base_salary=7500000),
        Position(name='Trưởng Ca Phục Vụ', department_id=dept_map['Phục Vụ'],
                 base_salary=8500000),
        Position(name='Nhân Viên Phục Vụ', department_id=dept_map['Phục Vụ'],
                 base_salary=6500000),
        Position(name='Thủ Kho', department_id=dept_map['Kho Vận'],
                 base_salary=8000000),
    ]
    for p in positions:
        db.session.add(p)
    db.session.flush()

    pos_map = {p.name: p.id for p in positions}

    # Sample employees
    employees = [
        Employee(
            employee_code='NV001',
            full_name='Nguyễn Văn An',
            date_of_birth=date(1985, 3, 15),
            gender='Nam',
            phone='0901234567',
            email='an.nguyen@thealley.vn',
            address='123 Nguyễn Huệ, Q1, TP.HCM',
            department_id=dept_map['Ban Giám Đốc'],
            position_id=pos_map['Giám Đốc'],
            hire_date=date(2020, 1, 1),
            status='active',
        ),
        Employee(
            employee_code='NV002',
            full_name='Trần Thị Bích',
            date_of_birth=date(1990, 7, 20),
            gender='Nữ',
            phone='0912345678',
            email='bich.tran@thealley.vn',
            address='45 Lê Lợi, Q1, TP.HCM',
            department_id=dept_map['Kế Toán'],
            position_id=pos_map['Kế Toán Trưởng'],
            hire_date=date(2020, 3, 1),
            status='active',
        ),
        Employee(
            employee_code='NV003',
            full_name='Lê Minh Cường',
            date_of_birth=date(1995, 11, 10),
            gender='Nam',
            phone='0923456789',
            email='cuong.le@thealley.vn',
            address='78 Đinh Tiên Hoàng, Bình Thạnh, TP.HCM',
            department_id=dept_map['Pha Chế'],
            position_id=pos_map['Barista'],
            hire_date=date(2021, 6, 15),
            status='active',
        ),
        Employee(
            employee_code='NV004',
            full_name='Phạm Thị Duyên',
            date_of_birth=date(1998, 4, 5),
            gender='Nữ',
            phone='0934567890',
            email='duyen.pham@thealley.vn',
            address='22 Võ Thị Sáu, Q3, TP.HCM',
            department_id=dept_map['Phục Vụ'],
            position_id=pos_map['Nhân Viên Phục Vụ'],
            hire_date=date(2022, 2, 1),
            status='active',
        ),
        Employee(
            employee_code='NV005',
            full_name='Hoàng Văn Em',
            date_of_birth=date(1993, 9, 25),
            gender='Nam',
            phone='0945678901',
            email='em.hoang@thealley.vn',
            address='156 Hai Bà Trưng, Q3, TP.HCM',
            department_id=dept_map['Nhân Sự'],
            position_id=pos_map['Trưởng Phòng Nhân Sự'],
            hire_date=date(2020, 5, 10),
            status='active',
        ),
    ]
    for e in employees:
        db.session.add(e)

    db.session.commit()
